namespace CPMS.API.Projections;

public class ChargeTagReadModel
{
    public Guid Id { get; set; }
    public string TagId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool Blocked { get; set; }
}