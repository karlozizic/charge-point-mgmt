using CPMS.API.Entities;
using CPMS.API.Events.ChargePoint;
using CPMS.API.Events.Connector;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.UnitTests;

public class ChargePointConnectorTests
{
    private static ChargePoint NewChargePoint() =>
        new(Guid.NewGuid(), "CP-1", Guid.NewGuid(), 22.0, 0.0);

    [Fact]
    public void AddConnector_takes_the_status_time_from_the_event()
    {
        var chargePoint = NewChargePoint();
        chargePoint.ClearDomainEvents();

        chargePoint.AddConnector(1, "Connector 1");

        var @event = Assert.IsType<ConnectorAddedEvent>(Assert.Single(chargePoint.DomainEvents));
        var connector = Assert.Single(chargePoint.Connectors);
        Assert.Equal(@event.AddedAt, connector.LastStatusTime);
        Assert.Equal("Available", connector.Status);
    }

    [Fact]
    public void AddConnector_rejects_a_connector_id_that_is_already_taken()
    {
        var chargePoint = NewChargePoint();
        chargePoint.AddConnector(1, "Connector 1");

        Assert.Throws<BusinessRuleValidationException>(() => chargePoint.AddConnector(1, "Connector 1 again"));
    }

    [Fact]
    public void LogConnectorError_gives_every_error_its_own_id()
    {
        var chargePoint = NewChargePoint();
        chargePoint.AddConnector(1, "Connector 1");
        chargePoint.ClearDomainEvents();

        chargePoint.LogConnectorError(1, "OverCurrentFailure", "phase 2");
        chargePoint.LogConnectorError(1, "GroundFailure", "phase 1");

        var ids = chargePoint.DomainEvents.OfType<ConnectorErrorLoggedEvent>()
            .Select(e => e.ErrorId)
            .ToList();

        Assert.Equal(2, ids.Count);
        Assert.DoesNotContain(Guid.Empty, ids);
        Assert.Equal(2, ids.Distinct().Count());
    }

    [Fact]
    public void LogConnectorError_records_the_error_on_the_connector()
    {
        var chargePoint = NewChargePoint();
        chargePoint.AddConnector(1, "Connector 1");
        chargePoint.ClearDomainEvents();

        chargePoint.LogConnectorError(1, "OverCurrentFailure", "phase 2");

        var @event = Assert.IsType<ConnectorErrorLoggedEvent>(Assert.Single(chargePoint.DomainEvents));
        var error = Assert.Single(chargePoint.Connectors.Single().Errors);
        Assert.Equal("OverCurrentFailure", error.ErrorCode);
        Assert.Equal(@event.Timestamp, error.Timestamp);
    }
}
