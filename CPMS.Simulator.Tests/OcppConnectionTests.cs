using System.Text.Json;
using CPMS.Simulator.Ocpp;

namespace CPMS.Simulator.Tests;

public class OcppConnectionTests
{
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static (FakeTransport, OcppConnection, Task, CancellationTokenSource) Running(
        TimeSpan? timeout = null)
    {
        var transport = new FakeTransport();
        var connection = new OcppConnection(transport, timeout);
        var cts = new CancellationTokenSource();
        return (transport, connection, connection.RunAsync(cts.Token), cts);
    }

    [Fact]
    public async Task A_call_goes_out_as_a_four_element_frame()
    {
        var (transport, connection, _, cts) = Running();

        var call = connection.CallAsync("Heartbeat", new { }, cts.Token);
        var frame = Parse(await transport.WaitForSentAsync(1));

        Assert.Equal(4, frame.GetArrayLength());
        Assert.Equal(2, frame[0].GetInt32());
        Assert.Equal("Heartbeat", frame[2].GetString());

        transport.Deliver($$"""[3,"{{frame[1].GetString()}}",{"currentTime":"now"}]""");
        Assert.Equal("now", (await call).GetProperty("currentTime").GetString());

        await cts.CancelAsync();
    }

    [Fact]
    public async Task Every_call_uses_a_fresh_unique_id()
    {
        var (transport, connection, _, cts) = Running();

        _ = connection.CallAsync("Heartbeat", new { }, cts.Token);
        _ = connection.CallAsync("Heartbeat", new { }, cts.Token);
        await transport.WaitForSentAsync(2);

        var first = Parse(transport.Sent[0])[1].GetString();
        var second = Parse(transport.Sent[1])[1].GetString();
        Assert.NotEqual(first, second);

        await cts.CancelAsync();
    }

    [Fact]
    public async Task Answers_reach_the_call_that_asked_even_when_they_arrive_out_of_order()
    {
        var (transport, connection, _, cts) = Running();

        var first = connection.CallAsync("StartTransaction", new { connectorId = 1 }, cts.Token);
        var second = connection.CallAsync("StartTransaction", new { connectorId = 2 }, cts.Token);
        await transport.WaitForSentAsync(2);

        var firstId = Parse(transport.Sent[0])[1].GetString();
        var secondId = Parse(transport.Sent[1])[1].GetString();

        transport.Deliver($$"""[3,"{{secondId}}",{"transactionId":222}]""");
        transport.Deliver($$"""[3,"{{firstId}}",{"transactionId":111}]""");

        Assert.Equal(111, (await first).GetProperty("transactionId").GetInt32());
        Assert.Equal(222, (await second).GetProperty("transactionId").GetInt32());

        await cts.CancelAsync();
    }

    [Fact]
    public async Task A_CALLERROR_faults_the_call_it_answers()
    {
        var (transport, connection, _, cts) = Running();

        var call = connection.CallAsync("MeterValues", new { }, cts.Token);
        var id = Parse(await transport.WaitForSentAsync(1))[1].GetString();
        transport.Deliver($$"""[4,"{{id}}","FormationViolation","bad payload",{}]""");

        var error = await Assert.ThrowsAsync<OcppCallErrorException>(() => call);
        Assert.Equal("FormationViolation", error.ErrorCode);

        await cts.CancelAsync();
    }

    [Fact]
    public async Task An_unanswered_call_times_out()
    {
        var (_, connection, _, cts) = Running(TimeSpan.FromMilliseconds(120));

        await Assert.ThrowsAsync<TimeoutException>(
            () => connection.CallAsync("Heartbeat", new { }, cts.Token));

        await cts.CancelAsync();
    }

    [Fact]
    public async Task An_answer_for_an_unknown_id_is_dropped()
    {
        var (transport, connection, _, cts) = Running();

        transport.Deliver("""[3,"nobody-is-waiting",{}]""");
        await Task.Delay(50);

        var call = connection.CallAsync("Heartbeat", new { }, cts.Token);
        var id = Parse(await transport.WaitForSentAsync(1))[1].GetString();
        transport.Deliver($$"""[3,"{{id}}",{"ok":true}]""");

        Assert.True((await call).GetProperty("ok").GetBoolean());

        await cts.CancelAsync();
    }

    [Fact]
    public async Task A_closed_transport_faults_the_calls_still_waiting()
    {
        var (transport, connection, run, cts) = Running();

        var call = connection.CallAsync("Heartbeat", new { }, cts.Token);
        await transport.WaitForSentAsync(1);
        transport.Close();

        await Assert.ThrowsAsync<IOException>(() => call);
        await run;
    }

    [Fact]
    public async Task A_server_call_is_answered_by_the_handler()
    {
        var (transport, connection, _, cts) = Running();
        connection.OnCall = (action, _) => action == "Reset" ? new { status = "Accepted" } : null;

        transport.Deliver("""[2,"srv-1","Reset",{"type":"Soft"}]""");
        var reply = Parse(await transport.WaitForSentAsync(1));

        Assert.Equal(3, reply[0].GetInt32());
        Assert.Equal("srv-1", reply[1].GetString());
        Assert.Equal("Accepted", reply[2].GetProperty("status").GetString());

        await cts.CancelAsync();
    }

    [Fact]
    public async Task An_action_the_handler_refuses_gets_a_five_element_CALLERROR()
    {
        var (transport, connection, _, cts) = Running();
        connection.OnCall = (_, _) => null;

        transport.Deliver("""[2,"srv-2","TriggerMessage",{}]""");
        var reply = Parse(await transport.WaitForSentAsync(1));

        Assert.Equal(5, reply.GetArrayLength());
        Assert.Equal(4, reply[0].GetInt32());
        Assert.Equal("srv-2", reply[1].GetString());
        Assert.Equal("NotImplemented", reply[2].GetString());

        await cts.CancelAsync();
    }

    [Fact]
    public async Task A_handler_that_throws_answers_InternalError_instead_of_killing_the_loop()
    {
        var (transport, connection, _, cts) = Running();
        connection.OnCall = (_, _) => throw new InvalidOperationException("boom");

        transport.Deliver("""[2,"srv-3","Reset",{}]""");
        var reply = Parse(await transport.WaitForSentAsync(1));

        Assert.Equal("InternalError", reply[2].GetString());

        connection.OnCall = (_, _) => new { status = "Accepted" };
        transport.Deliver("""[2,"srv-4","Reset",{}]""");
        Assert.Equal("srv-4", Parse(await transport.WaitForSentAsync(2))[1].GetString());

        await cts.CancelAsync();
    }

    [Fact]
    public async Task Rubbish_on_the_wire_is_skipped()
    {
        var (transport, connection, _, cts) = Running();

        transport.Deliver("not json at all");
        transport.Deliver("""{"not":"an array"}""");
        transport.Deliver("""[2]""");
        transport.Deliver("""["2","id",{}]""");
        transport.Deliver("""[3,42,{}]""");
        transport.Deliver("""[4,"id",{},{}]""");

        var call = connection.CallAsync("Heartbeat", new { }, cts.Token);
        var id = Parse(await transport.WaitForSentAsync(1))[1].GetString();
        transport.Deliver($$"""[3,"{{id}}",{"ok":true}]""");

        Assert.True((await call).GetProperty("ok").GetBoolean());

        await cts.CancelAsync();
    }
}
