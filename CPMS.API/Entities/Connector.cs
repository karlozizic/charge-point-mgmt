using CPMS.API.Events.Connector;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class Connector : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Status { get; private set; }
    public DateTime? LastStatusTime { get; private set; }

    private readonly List<ConnectorError> _errors = new();
    public IReadOnlyCollection<ConnectorError> Errors => _errors.AsReadOnly();

    private Connector()
    {
    }

    public Connector(int id, string name, DateTime? addedAt)
    {
        Id = id;
        Name = name;
        Status = "Available";
        LastStatusTime = addedAt;
    }

    public void UpdateStatus(string status, DateTime timestamp)
    {
        Status = status;
        LastStatusTime = timestamp;
    }

    public void LogError(string errorCode, string info, DateTime timestamp)
    {
        _errors.Add(new ConnectorError(errorCode, info, timestamp));
    }
}
