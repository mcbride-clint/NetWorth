namespace NetWorth.Domain;

public class LifeInsurancePlan
{
    public string CompanyPolicyNumber { get; set; }
    public string TermType { get; set; }
    public string Owner { get; set; }
    public string Insured { get; set; }
    public decimal DeathBenefit { get; set; }
    public string Beneficiary { get; set; }
    public DateTime? InceptionDate { get; set; }
    public string ContactPersonPhoneNumber { get; set; }
}
