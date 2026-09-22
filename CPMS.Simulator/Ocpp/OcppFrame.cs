using System.Text.Json;

namespace CPMS.Simulator.Ocpp;

public enum OcppMessageType
{
    Call = 2,
    CallResult = 3,
    CallError = 4
}

// One OCPP 1.6-J frame. Call carries Action and Payload; CallResult carries Payload;
// CallError carries ErrorCode and ErrorDescription.
public sealed record OcppFrame(
    OcppMessageType Type,
    string UniqueId,
    string? Action,
    JsonElement Payload,
    string? ErrorCode,
    string? ErrorDescription)
{
    private static readonly JsonElement Empty = JsonDocument.Parse("{}").RootElement.Clone();

    public static OcppFrame Call(string uniqueId, string action, JsonElement payload)
        => new(OcppMessageType.Call, uniqueId, action, payload, null, null);

    public static OcppFrame Result(string uniqueId, JsonElement payload)
        => new(OcppMessageType.CallResult, uniqueId, null, payload, null, null);

    public static OcppFrame Error(string uniqueId, string errorCode, string description)
        => new(OcppMessageType.CallError, uniqueId, null, Empty, errorCode, description);

    public string Serialize() => Type switch
    {
        OcppMessageType.Call => JsonSerializer.Serialize(new object?[] { 2, UniqueId, Action, Payload }),
        OcppMessageType.CallResult => JsonSerializer.Serialize(new object?[] { 3, UniqueId, Payload }),
        OcppMessageType.CallError => JsonSerializer.Serialize(
            new object?[] { 4, UniqueId, ErrorCode, ErrorDescription, new Dictionary<string, string>() }),
        _ => throw new InvalidOperationException($"Cannot serialize {Type}")
    };

    public static bool TryParse(string frame, out OcppFrame parsed)
    {
        parsed = null!;
        try
        {
            using var document = JsonDocument.Parse(frame);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 3) return false;

            var type = (OcppMessageType)root[0].GetInt32();
            var uniqueId = root[1].GetString();
            if (string.IsNullOrEmpty(uniqueId)) return false;

            parsed = type switch
            {
                OcppMessageType.Call when root.GetArrayLength() >= 4 =>
                    Call(uniqueId, root[2].GetString() ?? "", root[3].Clone()),
                OcppMessageType.CallResult =>
                    Result(uniqueId, root[2].Clone()),
                OcppMessageType.CallError when root.GetArrayLength() >= 4 =>
                    Error(uniqueId, root[2].GetString() ?? "", root[3].GetString() ?? ""),
                _ => null!
            };
            return parsed is not null;
        }
        // JsonException is malformed text; InvalidOperationException is well-formed JSON of the wrong
        // shape, such as a message type sent as a string. Both are a frame to skip, not a reason to
        // end the receive loop.
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            return false;
        }
    }
}
