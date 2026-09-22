using System.Net.Http.Json;
using System.Text.Json;

namespace CPMS.Simulator;

public sealed record SeedResult(int ChargePointsCreated, int TagsCreated, int PricingAssigned);

// The gateway rejects BootNotification for a charge point the API does not know, refuses
// StartTransaction for a tag it has not authorized, and billing needs a pricing group on the charge
// point. Without all three the run measures silence or fills the log with billing failures.
public sealed class Seeder(HttpClient http)
{
    // Reserved for the browser simulator in simulator/, so a hand-driven charger never collides with
    // a fleet member. The gateway closes the older socket when one id connects twice.
    public const string ManualChargerId = "CP-MANUAL-1";
    public const string ManualTagId = "TAG-MANUAL";

    private const string SeedName = "Simulator";
    private const int PageSize = 1000;

    public async Task<SeedResult> EnsureAsync(
        IEnumerable<string> chargerIds, IEnumerable<string> tagIds, CancellationToken ct)
    {
        var wanted = chargerIds.Append(ManualChargerId).ToArray();
        var locationId = await EnsureLocationAsync(ct);

        var knownTags = await TagsAsync(ct);
        var tagsCreated = 0;
        foreach (var tagId in tagIds.Append(ManualTagId).Distinct())
        {
            if (knownTags.Contains(tagId)) continue;
            var response = await http.PostAsJsonAsync("api/ChargeTags", new { tagId }, ct);
            response.EnsureSuccessStatusCode();
            tagsCreated++;
        }

        // One request at a time, on purpose. Every charge point created here is appended to the same
        // Location stream by an event handler after the charge point itself has committed, and every
        // pricing assignment is appended to the same PricingGroup stream. Two of either in flight at
        // once load the same stream version and the second append fails - the API has no optimistic
        // concurrency and no retry. For the charge point that surfaces as a 500 for a row that exists.
        var existing = await ChargePointsAsync(ct);
        var created = 0;
        foreach (var id in wanted.Where(id => !existing.ContainsKey(id)))
        {
            var response = await http.PostAsJsonAsync("api/ChargePoints",
                new { ocppChargerId = id, locationId, maxPower = 22 }, ct);
            response.EnsureSuccessStatusCode();
            existing[id] = (await response.Content.ReadFromJsonAsync<string>(cancellationToken: ct))!;
            created++;
        }

        var (groupId, priced) = await EnsurePricingGroupAsync(ct);
        var assigned = 0;
        foreach (var id in wanted)
        {
            if (!existing.TryGetValue(id, out var chargePointId) || !priced.Add(chargePointId)) continue;
            var response = await http.PostAsync(
                $"api/Pricing/groups/{groupId}/assign-chargepoint/{chargePointId}", null, ct);
            response.EnsureSuccessStatusCode();
            assigned++;
        }

        return new SeedResult(created, tagsCreated, assigned);
    }

    // Some endpoints answer a bare array, others a page: {"items":[...]}.
    private static IEnumerable<JsonElement> Rows(JsonElement body)
    {
        if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("items", out var items)) body = items;
        return body.ValueKind == JsonValueKind.Array ? body.EnumerateArray() : [];
    }

    private static string? NameOf(JsonElement row)
        => row.TryGetProperty("name", out var name) ? name.GetString() : null;

    private async Task<string> EnsureLocationAsync(CancellationToken ct)
    {
        // Locations is paged and defaults to ten, which would hide the seeded one and create a new
        // location on every run.
        var locations = await http.GetFromJsonAsync<JsonElement>($"api/Locations?pageSize={PageSize}", ct);
        foreach (var location in Rows(locations))
        {
            if (NameOf(location) == SeedName) return location.GetProperty("id").GetString()!;
        }

        var response = await http.PostAsJsonAsync("api/Locations", new
        {
            name = SeedName, address = "-", city = "-", postalCode = "-", country = "HR",
            latitude = 45.81, longitude = 15.98, description = "load test"
        }, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<string>(cancellationToken: ct))!;
    }

    private async Task<HashSet<string>> TagsAsync(CancellationToken ct)
    {
        var tags = await http.GetFromJsonAsync<JsonElement>("api/ChargeTags", ct);
        return Rows(tags).Select(tag => tag.GetProperty("tagId").GetString()!).ToHashSet();
    }

    private async Task<Dictionary<string, string>> ChargePointsAsync(CancellationToken ct)
    {
        var chargePoints = await http.GetFromJsonAsync<JsonElement>("api/ChargePoints", ct);

        // The API does not enforce a unique ocppChargerId, and the gateway routes by it. Take the
        // first of any duplicate rather than failing the seed over someone else's data.
        return Rows(chargePoints)
            .GroupBy(cp => cp.GetProperty("ocppChargerId").GetString()!)
            .ToDictionary(group => group.Key, group => group.First().GetProperty("id").GetString()!);
    }

    private async Task<(string GroupId, HashSet<string> AlreadyPriced)> EnsurePricingGroupAsync(CancellationToken ct)
    {
        var groups = await http.GetFromJsonAsync<JsonElement>("api/Pricing/groups", ct);
        var priced = new HashSet<string>();
        string? groupId = null;

        foreach (var group in Rows(groups))
        {
            if (group.TryGetProperty("chargePointIds", out var ids))
            {
                foreach (var id in ids.EnumerateArray()) priced.Add(id.GetString()!);
            }
            if (NameOf(group) == SeedName) groupId = group.GetProperty("id").GetString();
        }

        if (groupId is not null) return (groupId, priced);

        var response = await http.PostAsJsonAsync("api/Pricing/groups",
            new { name = SeedName, basePrice = 1.0, pricePerKwh = 0.3, currency = "EUR" }, ct);
        response.EnsureSuccessStatusCode();
        return ((await response.Content.ReadFromJsonAsync<string>(cancellationToken: ct))!, priced);
    }
}
