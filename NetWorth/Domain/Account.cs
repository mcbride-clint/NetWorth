namespace NetWorth.Domain;

public record Account
{
    public string? Name { get; set; }
    public decimal? Balance { get; set; }
    public decimal? SpouseBalance { get; set; }
    public decimal? JointBalance { get; set; }
    public decimal Total => Balance ?? 0 + SpouseBalance ?? 0 + JointBalance ?? 0; 
}
