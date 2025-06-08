namespace CPMS.API.Projections;

public class PricingGroupReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PricePerKwh { get; set; }
    public string Currency { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> ChargePointIds { get; set; } = new();
}
