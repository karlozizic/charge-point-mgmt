namespace CPMS.Simulator.Tests;

public class MetricsTests
{
    [Fact]
    public void An_empty_run_reports_nothing()
    {
        var report = new Metrics().Snapshot();

        Assert.Equal(0, report.Messages);
        Assert.Empty(report.Actions);
    }

    [Fact]
    public void Successes_and_failures_are_counted_per_action()
    {
        var metrics = new Metrics();
        metrics.Record("Heartbeat", TimeSpan.FromMilliseconds(5), Outcome.Ok);
        metrics.Record("Heartbeat", TimeSpan.FromMilliseconds(5), Outcome.Failed);
        metrics.Record("MeterValues", TimeSpan.FromMilliseconds(5), Outcome.Ok);

        var report = metrics.Snapshot();

        Assert.Equal(3, report.Messages);
        var heartbeat = report.Actions.Single(a => a.Action == "Heartbeat");
        Assert.Equal(1, heartbeat.Ok);
        Assert.Equal(1, heartbeat.Failed);
    }

    [Fact]
    public void A_no_from_the_CSMS_is_counted_apart_from_a_failure()
    {
        var metrics = new Metrics();
        metrics.Record("BootNotification", TimeSpan.FromMilliseconds(5), Outcome.Ok);
        metrics.Record("BootNotification", TimeSpan.FromMilliseconds(5), Outcome.Rejected);
        metrics.Record("BootNotification", TimeSpan.FromMilliseconds(5), Outcome.Failed);

        var boot = metrics.Snapshot().Actions.Single();

        Assert.Equal(1, boot.Ok);
        Assert.Equal(1, boot.Rejected);
        Assert.Equal(1, boot.Failed);
    }

    [Fact]
    public void Connect_attempts_are_listed_but_not_counted_as_messages()
    {
        var metrics = new Metrics();
        metrics.Record(Metrics.ConnectAction, TimeSpan.FromMilliseconds(5), Outcome.Ok);
        metrics.Record(Metrics.ConnectAction, TimeSpan.FromMilliseconds(5), Outcome.Failed);
        metrics.Record("Heartbeat", TimeSpan.FromMilliseconds(5), Outcome.Ok);

        var report = metrics.Snapshot();

        Assert.Equal(1, report.Messages);
        Assert.Contains(report.Actions, a => a.Action == Metrics.ConnectAction);
    }

    [Fact]
    public void The_peak_of_connected_chargers_is_reported()
    {
        var metrics = new Metrics(chargers: 3);
        metrics.Connected();
        metrics.Connected();
        metrics.Disconnected();
        metrics.Connected();

        var report = metrics.Snapshot();

        Assert.Equal(2, report.PeakConnected);
        Assert.Equal(3, report.Chargers);
        Assert.Contains("peak 2 of 3 connected", report.ToString());
        Assert.Contains("1 chargers never connected", report.ToString());
    }

    [Fact]
    public void Percentiles_come_from_the_recorded_latencies()
    {
        var metrics = new Metrics();
        for (var ms = 1; ms <= 100; ms++) metrics.Record("StartTransaction", TimeSpan.FromMilliseconds(ms), Outcome.Ok);

        var action = metrics.Snapshot().Actions.Single();

        Assert.Equal(50, action.P50Ms, 3);
        Assert.Equal(95, action.P95Ms, 3);
        Assert.Equal(99, action.P99Ms, 3);
    }

    [Fact]
    public void The_report_prints_a_line_per_action()
    {
        var metrics = new Metrics();
        metrics.Record("Heartbeat", TimeSpan.FromMilliseconds(5), Outcome.Ok);
        metrics.Record("MeterValues", TimeSpan.FromMilliseconds(5), Outcome.Ok);

        var text = metrics.Snapshot().ToString();

        Assert.Contains("Heartbeat", text);
        Assert.Contains("MeterValues", text);
        Assert.Contains("msg/s", text);
    }
}
