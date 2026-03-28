
namespace NetWorth.Domain;

public class Account
{
    public string? DefinitionId { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public double? Balance { get; set; }
    public double? SpouseBalance { get; set; }
    public double? JointBalance { get; set; }
    public string? Notes { get; set; }
    public double? AnnualContribution { get; set; }
    public double? InterestRate { get; set; }  // decimal fraction, e.g. 0.0675 = 6.75%
    public double Total => (Balance ?? 0) + (SpouseBalance ?? 0) + (JointBalance ?? 0);

    internal Account Clone()
    {
        return new Account()
        {
            DefinitionId = DefinitionId,
            Type = Type,
            Name = Name,
            Balance = Balance,
            SpouseBalance = SpouseBalance,
            JointBalance = JointBalance,
            Notes = Notes,
            AnnualContribution = AnnualContribution,
            InterestRate = InterestRate
        };
    }
}
