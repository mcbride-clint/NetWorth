namespace NetWorth.Domain;

public class YearSummary
{
    public int? Year => AsOfDate?.Year;
    public DateTime? AsOfDate { get; set; }
    public decimal HouseholdIncome { get; set; }
    public List<Account> CashAccounts { get; set; } = [];
    public List<Account> AfterTaxInvestmentAccounts { get; set; } = [];
    public List<Account> TaxDeferredInvestmentAccounts { get; set; } = [];
    public List<Account> TaxFreeInvestmentAccounts { get; set; } = [];
    public List<Account> BusinessInterests { get; set; } = [];
    public List<Account> Property { get; set; } = [];
    public List<Account> Liabilities { get; set; } = [];
    public List<Account> DeferredTaxes { get; set; } = [];


    internal YearSummary Clone()
    {
        return new YearSummary() {
            AsOfDate = AsOfDate,
            HouseholdIncome = HouseholdIncome,
            CashAccounts = CashAccounts.Select(a => a with { }).ToList(),
            AfterTaxInvestmentAccounts = AfterTaxInvestmentAccounts.Select(a => a with { }).ToList(),
            TaxDeferredInvestmentAccounts = TaxDeferredInvestmentAccounts.Select(a => a with { }).ToList(),
            TaxFreeInvestmentAccounts = TaxFreeInvestmentAccounts.Select(a => a with { }).ToList(),
            BusinessInterests = BusinessInterests.Select(a => a with { }).ToList(),
            Property = Property.Select(a => a with { }).ToList(),
            Liabilities = Liabilities.Select(a => a with { }).ToList(),
            DeferredTaxes = DeferredTaxes.Select(a => a with { }).ToList(),
        };
    }
}
