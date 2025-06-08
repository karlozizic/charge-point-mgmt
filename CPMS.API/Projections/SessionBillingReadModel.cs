namespace CPMS.API.Projections;

public class SessionBillingReadModel
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid PricingGroupId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal EnergyAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeSessionId { get; set; } 
    public string PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}