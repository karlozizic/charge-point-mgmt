using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CPMS.Proxy.Models;

public interface IRawMessageSink
{
    string ExtensionName { get; }

    bool InitializeExtension(ILoggerFactory logFactory, IConfiguration configuration);

    void ReceiveIncomingMessage(string ocppVersion, string chargePointId, IOcppMessage rawMessage);

    void ReceiveOutgoingMessage(string ocppVersion, string chargePointId, IOcppMessage rawMessage);
}