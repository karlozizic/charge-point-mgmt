using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using CPMS.Simulator.Ocpp;

namespace CPMS.Simulator;

public sealed record ChargerOptions(
    Uri GatewayUrl,
    string IdTag,
    TimeSpan SessionDuration,
    TimeSpan MeterInterval,
    TimeSpan IdleBetweenSessions,
    TimeSpan StopGrace,
    double BatteryCapacityWh,
    double PowerW);

// One charger, one socket, one Task. The protocol flow is written in order - boot, then sessions of
// authorize, start, meter values, stop - so a charger's state is simply where this code is. A
// MeterValues outside a transaction cannot happen: the meter loop only exists inside the charging
// block. The only remembered state is the open transaction.
public sealed class ChargerClient(
    string chargerId,
    double initialSoCPercent,
    ChargerOptions options,
    Func<Uri, CancellationToken, Task<IOcppTransport>> connect,
    Metrics metrics)
{
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(30);

    private OcppConnection _connection = null!;
    private BatteryModel _battery = new(options.BatteryCapacityWh, initialSoCPercent, options.PowerW);
    private int? _transactionId;

    public Uri Url => new($"{options.GatewayUrl.ToString().TrimEnd('/')}/{chargerId}");

    public async Task RunAsync(CancellationToken ct)
    {
        var attempt = 0;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var transport = await ConnectAsync(ct);
                attempt = 0;
                metrics.Connected();
                try
                {
                    await DriveAsync(transport, ct);
                }
                finally
                {
                    metrics.Disconnected();
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch
            {
                // Any failure is a dropped connection as far as the charger is concerned.
            }

            _transactionId = null;
            if (ct.IsCancellationRequested) return;

            var delay = TimeSpan.FromMilliseconds(Math.Min(
                BaseDelay.TotalMilliseconds * Math.Pow(2, attempt++), MaxDelay.TotalMilliseconds));

            // A cancelled run is not a failure. Throwing here would fault the whole fleet's
            // Task.WhenAll and lose the report.
            try
            {
                await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    // Recorded so the report can say how many of the fleet were ever actually connected. A gateway
    // that refuses half the connections otherwise produces a report that reads better than a healthy
    // run: fewer messages, same latencies, no failures.
    private async Task<IOcppTransport> ConnectAsync(CancellationToken ct)
    {
        var clock = Stopwatch.StartNew();
        try
        {
            var transport = await connect(Url, ct);
            metrics.Record("Connect", clock.Elapsed, Outcome.Ok);
            return transport;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            metrics.Record("Connect", clock.Elapsed, Outcome.Failed);
            throw;
        }
    }

    private async Task DriveAsync(IOcppTransport transport, CancellationToken ct)
    {
        // Two signals, not one. `work` stops the charger starting anything new; `socket` tears the
        // connection down. The goodbye StopTransaction needs the connection to outlive the work.
        using var work = CancellationTokenSource.CreateLinkedTokenSource(ct);
        using var socket = new CancellationTokenSource();

        _connection = new OcppConnection(transport) { OnCall = AnswerServerCall };
        var pump = _connection.RunAsync(socket.Token);

        try
        {
            var heartbeatInterval = await BootUntilAcceptedAsync(work.Token);
            await SendStatusAsync("Available", work.Token);

            var sessions = SessionLoopAsync(work.Token);
            await Task.WhenAny(HeartbeatLoopAsync(heartbeatInterval, work.Token), sessions, pump);

            // Whatever ended first, stop the work and let the goodbye land on a live socket.
            await work.CancelAsync();
            await Settle(sessions, options.StopGrace + TimeSpan.FromSeconds(1));
        }
        finally
        {
            await work.CancelAsync();
            await socket.CancelAsync();
            await Settle(pump, TimeSpan.FromSeconds(1));
        }
    }

    // A charge point sends nothing but answers until its BootNotification is Accepted, and retries
    // after the interval the CSMS names (OCPP 1.6 4.2.1).
    private async Task<TimeSpan> BootUntilAcceptedAsync(CancellationToken ct)
    {
        while (true)
        {
            var answer = await CallAsync("BootNotification", new
            {
                chargePointVendor = "CPMS",
                chargePointModel = "Simulator",
                chargePointSerialNumber = chargerId
            }, ct, Accepted) ?? throw new IOException("BootNotification was not answered");

            var interval = answer.TryGetProperty("interval", out var i) ? Math.Max(i.GetInt32(), 1) : 300;
            if (answer.TryGetProperty("status", out var status) && status.GetString() == "Accepted")
            {
                return TimeSpan.FromSeconds(interval);
            }

            await Task.Delay(TimeSpan.FromSeconds(interval), ct);
        }
    }

    private async Task SessionLoopAsync(CancellationToken ct)
    {
        try
        {
            while (true)
            {
                // A stop that failed leaves the transaction open on the CSMS. Starting another would
                // leave the first one in Started for ever, so the charger retries the stop instead.
                if (_transactionId is not null)
                {
                    await StopAsync(ct);
                }
                else if (await AuthorizeAsync(ct) && await StartAsync(ct))
                {
                    _battery = new BatteryModel(options.BatteryCapacityWh, initialSoCPercent, options.PowerW);
                    await SendStatusAsync("Charging", ct);

                    // The last step is cut to the time left, so a session ends when --session says
                    // and the final MeterValues carries the energy of the time actually charged.
                    var until = DateTime.UtcNow + options.SessionDuration;
                    while (_transactionId is not null)
                    {
                        var remaining = until - DateTime.UtcNow;
                        if (remaining <= TimeSpan.Zero) break;

                        var step = remaining < options.MeterInterval ? remaining : options.MeterInterval;
                        await Task.Delay(step, ct);
                        _battery.Advance(step);
                        await MeterAsync(ct);
                    }

                    await StopAsync(ct);
                }

                await Task.Delay(options.IdleBetweenSessions, ct);
            }
        }
        catch (OperationCanceledException)
        {
        }

        if (_transactionId is null) return;
        using var grace = new CancellationTokenSource(options.StopGrace);
        await Settle(StopAsync(grace.Token), options.StopGrace);
    }

    private async Task HeartbeatLoopAsync(TimeSpan interval, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(interval, ct);
                await CallAsync("Heartbeat", new { }, ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<bool> AuthorizeAsync(CancellationToken ct)
    {
        var answer = await CallAsync("Authorize", new { idTag = options.IdTag }, ct, TagAccepted);
        return answer is not null && StatusOf(answer.Value) == "Accepted";
    }

    private async Task<bool> StartAsync(CancellationToken ct)
    {
        var answer = await CallAsync("StartTransaction", new
        {
            connectorId = 1,
            idTag = options.IdTag,
            meterStart = 0,
            timestamp = Now()
        }, ct, TagAccepted);

        if (answer is null
            || StatusOf(answer.Value) != "Accepted"
            || !answer.Value.TryGetProperty("transactionId", out var transactionId))
        {
            return false;
        }

        _transactionId = transactionId.GetInt32();
        return true;
    }

    private Task MeterAsync(CancellationToken ct)
        => CallAsync("MeterValues", new
        {
            connectorId = 1,
            transactionId = _transactionId ?? 0,
            meterValue = new[]
            {
                new
                {
                    timestamp = Now(),
                    sampledValue = new object[]
                    {
                        new { value = Math.Round(_battery.EnergyWh).ToString("F0"), measurand = "Energy.Active.Import.Register", unit = "Wh", context = "Sample.Periodic" },
                        new { value = options.PowerW.ToString("F0"), measurand = "Power.Active.Import", unit = "W", context = "Sample.Periodic" },
                        new { value = Math.Round(_battery.SoCPercent).ToString("F0"), measurand = "SoC", unit = "Percent", context = "Sample.Periodic" }
                    }
                }
            }
        }, ct);

    private async Task StopAsync(CancellationToken ct)
    {
        var answer = await CallAsync("StopTransaction", new
        {
            transactionId = _transactionId ?? 0,
            idTag = options.IdTag,
            meterStop = (int)Math.Round(_battery.EnergyWh),
            timestamp = Now(),
            reason = "Local"
        }, ct);

        if (answer is null) return;
        _transactionId = null;
        await SendStatusAsync("Available", ct);
    }

    private Task SendStatusAsync(string status, CancellationToken ct)
        => CallAsync("StatusNotification", new
        {
            connectorId = 1,
            status,
            errorCode = "NoError",
            timestamp = Now()
        }, ct);

    // `accepted` separates a CSMS that answered "no" from one that did not answer at all. Actions
    // with no yes/no in their answer pass null.
    private async Task<JsonElement?> CallAsync(
        string action, object payload, CancellationToken ct, Func<JsonElement, bool>? accepted = null)
    {
        var clock = Stopwatch.StartNew();
        try
        {
            var answer = await _connection.CallAsync(action, payload, ct);
            metrics.Record(action, clock.Elapsed,
                accepted is null || accepted(answer) ? Outcome.Ok : Outcome.Rejected);
            return answer;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            metrics.Record(action, clock.Elapsed, Outcome.Failed);
            return null;
        }
    }

    private static bool Accepted(JsonElement answer)
        => answer.TryGetProperty("status", out var status) && status.GetString() == "Accepted";

    private static bool TagAccepted(JsonElement answer) => StatusOf(answer) == "Accepted";

    private object? AnswerServerCall(string action, JsonElement payload) => action switch
    {
        "Reset" => new { status = "Accepted" },
        "RemoteStartTransaction" => new { status = _transactionId is null ? "Accepted" : "Rejected" },
        "RemoteStopTransaction" => new { status = _transactionId is null ? "Rejected" : "Accepted" },
        "UnlockConnector" => new { status = "Unlocked" },
        "ChangeAvailability" => new { status = _transactionId is null ? "Accepted" : "Scheduled" },
        "ChangeConfiguration" => new { status = "Accepted" },
        "GetConfiguration" => new { configurationKey = Array.Empty<object>() },
        _ => null
    };

    private static async Task Settle(Task task, TimeSpan within)
    {
        try
        {
            await task.WaitAsync(within);
        }
        catch (Exception exception) when (exception is TimeoutException or OperationCanceledException)
        {
        }
    }

    private static string? StatusOf(JsonElement answer)
        => answer.TryGetProperty("idTagInfo", out var info) && info.TryGetProperty("status", out var status)
            ? status.GetString()
            : null;

    // Invariant on purpose: in a custom format ':' is the current culture's time separator, and a
    // Finnish machine would otherwise send 10.15.30Z and get FormationViolation for every message.
    private static string Now() => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
}
