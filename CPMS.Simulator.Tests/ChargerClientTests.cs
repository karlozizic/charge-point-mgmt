using System.Text.Json;
using CPMS.Simulator.Ocpp;

namespace CPMS.Simulator.Tests;

public class ChargerClientTests
{
    private static readonly TimeSpan Grace = TimeSpan.FromMilliseconds(500);

    private static ChargerOptions Options(TimeSpan? session = null) => new(
        GatewayUrl: new Uri("ws://localhost:5000/OCPP"),
        IdTag: "TAG-OK",
        SessionDuration: session ?? TimeSpan.FromMilliseconds(300),
        MeterInterval: TimeSpan.FromMilliseconds(100),
        IdleBetweenSessions: TimeSpan.FromMilliseconds(50),
        StopGrace: Grace,
        BatteryCapacityWh: 50_000,
        PowerW: 11_000);

    // A CSMS that answers whatever the charger asks. It reads the outbound channel, so it answers
    // with no delay and stops when the charger disposes its transport. `failWhen` picks, by action
    // and by how many of that action it has seen, which calls get a CALLERROR instead.
    private sealed class Csms(
        FakeTransport transport,
        string bootStatus = "Accepted",
        string tagStatus = "Accepted",
        Func<string, int, bool>? failWhen = null)
    {
        private readonly Dictionary<string, TaskCompletionSource> _awaited = new();
        private int _transactionId = 1000;

        public List<string> Actions { get; } = [];

        public async Task PumpAsync()
        {
            await foreach (var text in transport.SentFrames)
            {
                if (!OcppFrame.TryParse(text, out var frame) || frame.Type != OcppMessageType.Call) continue;

                Record(frame.Action!);
                var reply = failWhen?.Invoke(frame.Action!, Count(frame.Action!)) == true
                    ? OcppFrame.Error(frame.UniqueId, "InternalError", "made to fail")
                    : OcppFrame.Result(frame.UniqueId, Answer(frame.Action!));
                transport.Deliver(reply.Serialize());
            }
        }

        private JsonElement Answer(string action) => JsonSerializer.SerializeToElement(action switch
        {
            "BootNotification" => new { status = bootStatus, currentTime = "now", interval = 300 },
            "Authorize" => (object)new { idTagInfo = new { status = tagStatus } },
            "StartTransaction" => new { idTagInfo = new { status = tagStatus }, transactionId = ++_transactionId },
            _ => new { }
        });

        public int Count(string action)
        {
            lock (Actions) return Actions.Count(a => a == action);
        }

        public Task SeenAsync(string action, int count)
        {
            lock (Actions)
            {
                if (Actions.Count(a => a == action) >= count) return Task.CompletedTask;
                var waiter = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _awaited[$"{action}:{count}"] = waiter;
                return waiter.Task;
            }
        }

        private void Record(string action)
        {
            lock (Actions)
            {
                Actions.Add(action);
                var key = $"{action}:{Actions.Count(a => a == action)}";
                if (_awaited.Remove(key, out var waiter)) waiter.SetResult();
            }
        }

        public string Seen()
        {
            lock (Actions) return string.Join(", ", Actions);
        }
    }

    private static (Csms, Task, CancellationTokenSource) Start(
        string bootStatus = "Accepted",
        string tagStatus = "Accepted",
        TimeSpan? session = null,
        Func<string, int, bool>? failWhen = null)
    {
        var transport = new FakeTransport();
        var csms = new Csms(transport, bootStatus, tagStatus, failWhen);
        var cts = new CancellationTokenSource();
        var client = new ChargerClient("CP-1", 20, Options(session),
            (_, _) => Task.FromResult<IOcppTransport>(transport), new Metrics());

        _ = csms.PumpAsync();
        return (csms, client.RunAsync(cts.Token), cts);
    }

    private static Task Within(Task task, TimeSpan? limit = null)
        => task.WaitAsync(limit ?? TimeSpan.FromSeconds(10));

    [Fact]
    public void The_charger_id_is_appended_to_the_gateway_url()
    {
        var client = new ChargerClient("CP-1", 20, Options(), (_, _) => throw new NotSupportedException(), new Metrics());

        Assert.Equal("ws://localhost:5000/OCPP/CP-1", client.Url.ToString());
    }

    [Fact]
    public async Task A_charger_boots_authorizes_starts_meters_and_stops()
    {
        var (csms, _, cts) = Start();

        await Within(csms.SeenAsync("StopTransaction", 1));

        Assert.Equal(1, csms.Count("BootNotification"));
        Assert.True(csms.Count("Authorize") >= 1, csms.Seen());
        Assert.True(csms.Count("StartTransaction") >= 1, csms.Seen());
        Assert.True(csms.Count("MeterValues") >= 1, csms.Seen());
        Assert.True(csms.Count("StatusNotification") >= 2, csms.Seen());

        await cts.CancelAsync();
    }

    [Fact]
    public async Task Sessions_repeat_until_the_token_trips()
    {
        var (csms, run, cts) = Start();

        await Within(csms.SeenAsync("StartTransaction", 2));
        await cts.CancelAsync();

        await Within(run);
    }

    [Fact]
    public async Task Shutdown_stops_a_running_transaction()
    {
        // A session long enough that no natural stop can land first.
        var (csms, run, cts) = Start(session: TimeSpan.FromMinutes(1));

        await Within(csms.SeenAsync("MeterValues", 1));
        Assert.Equal(0, csms.Count("StopTransaction"));

        await cts.CancelAsync();
        await Within(run);

        Assert.Equal(1, csms.Count("StopTransaction"));
    }

    [Fact]
    public async Task A_failed_stop_is_retried_before_any_new_session_starts()
    {
        var (csms, _, cts) = Start(failWhen: (action, nth) => action == "StopTransaction" && nth == 1);

        await Within(csms.SeenAsync("StopTransaction", 2));

        // The first stop got a CALLERROR. Until the retry lands there is still one open transaction,
        // so there must not have been a second StartTransaction in between.
        Assert.Equal(1, csms.Count("StartTransaction"));

        await cts.CancelAsync();
    }

    [Fact]
    public async Task A_session_ends_on_time_even_when_the_meter_interval_is_longer()
    {
        var (csms, _, cts) = Start(session: TimeSpan.FromMilliseconds(150));
        // Options() sets a 100 ms meter interval: one full step, then a 50 ms step to the end.

        await Within(csms.SeenAsync("StopTransaction", 1));

        Assert.Equal(2, csms.Count("MeterValues"));

        await cts.CancelAsync();
    }

    [Fact]
    public async Task A_rejected_boot_keeps_the_charger_quiet()
    {
        var (csms, _, cts) = Start(bootStatus: "Rejected");

        await Within(csms.SeenAsync("BootNotification", 1));
        await Task.Delay(200);

        Assert.Equal(0, csms.Count("Authorize"));
        Assert.Equal(0, csms.Count("StartTransaction"));
        Assert.Equal(0, csms.Count("MeterValues"));
        Assert.Equal(0, csms.Count("StatusNotification"));

        await cts.CancelAsync();
    }

    [Fact]
    public async Task A_rejected_tag_never_reaches_StartTransaction()
    {
        var (csms, _, cts) = Start(tagStatus: "Blocked");

        await Within(csms.SeenAsync("Authorize", 2));

        Assert.Equal(0, csms.Count("StartTransaction"));
        Assert.Equal(0, csms.Count("MeterValues"));

        await cts.CancelAsync();
    }

    [Fact]
    public async Task A_failed_connection_is_retried_with_backoff()
    {
        var attempts = 0;
        var cts = new CancellationTokenSource();
        var client = new ChargerClient("CP-1", 20, Options(), (_, _) =>
        {
            Interlocked.Increment(ref attempts);
            throw new IOException("refused");
        }, new Metrics());

        var run = client.RunAsync(cts.Token);
        await Task.Delay(TimeSpan.FromMilliseconds(2500));
        await cts.CancelAsync();
        await Within(run);

        Assert.InRange(attempts, 2, 5);   // 1 s, 2 s backoff, not a spin
    }
}
