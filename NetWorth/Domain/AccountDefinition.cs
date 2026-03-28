
namespace NetWorth.Domain;

public class AccountDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "";
    /// <summary>Matches AssetClass.Category (e.g. "Cash", "Tax-Deferred Investment")</summary>
    public string AssetClassCategory { get; set; } = "";
    /// <summary>Sub-type from AssetClass.Presets (e.g. "401k - Pre-Tax")</summary>
    public string Type { get; set; } = "";
    /// <summary>"Primary", "Spouse", or "Joint"</summary>
    public string Owner { get; set; } = "Primary";
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>Annual interest rate as a decimal fraction, e.g. 0.0675 = 6.75%</summary>
    public double? InterestRate { get; set; }

    internal AccountDefinition Clone() => new()
    {
        Id = Id,
        Name = Name,
        AssetClassCategory = AssetClassCategory,
        Type = Type,
        Owner = Owner,
        Website = Website,
        Notes = Notes,
        IsActive = IsActive,
        InterestRate = InterestRate,
    };
}
