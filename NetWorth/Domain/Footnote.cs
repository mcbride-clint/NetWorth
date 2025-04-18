namespace NetWorth.Domain;

public class Footnote
{
    public List<LifeInsurancePlan> LifeInsurancePlans { get; set; } = new();
    public List<CollegeSavingsAccount> CollegeSavingsAccounts { get; set; } = new();
    public string RealEstateDetails { get; set; }
}

