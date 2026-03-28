
namespace NetWorth.Domain;

public class Account
{
    public string? DefinitionId { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public double? Balance { get; set; }
    /// <summary>Legacy field — migrated to Balance on load. Not shown in UI.</summary>
    public double? SpouseBalance { get; set; }
    /// <summary>Legacy field — migrated to Balance on load. Not shown in UI.</summary>
    public double? JointBalance { get; set; }
    public string? Notes { get; set; }
    public double? AnnualContribution { get; set; }
    /// <summary>Legacy field — migrated to AccountDefinition.InterestRate on load. Not shown in UI.</summary>
    public double? InterestRate { get; set; }
    public double Total => Balance ?? 0;

    internal Account Clone()
    {
        return new Account()
        {
            DefinitionId = DefinitionId,
            Type = Type,
            Name = Name,
            Balance = Balance,
            Notes = Notes,
            AnnualContribution = AnnualContribution,
            InterestRate = InterestRate,
        };
    }
}
