namespace NetWorth.Domain;

public class YearSummary
{
    public int? Year { get; set; }
    public decimal HouseholdIncome { get; set; }
    public List<Account> CashAccounts { get; set; } = [];
    public List<Account> AfterTaxInvestmentAccounts { get; set; } = [];
    public List<Account> TaxDeferredInvestmentAccounts { get; set; } = [];
    public List<Account> TaxFreeInvestmentAccounts { get; set; } = [];
    public List<Account> BusinessInterests { get; set; } = [];
    public List<Account> Property { get; set; } = [];
    public List<Account> Liabilities { get; set; } = [];
    public List<Account> DeferredTaxes { get; set; } = [];

    public double TotalLiquidAssets => CashAccounts.Sum(a => a.Total);
    public double TotalAssets => AfterTaxInvestmentAccounts.Sum(a => a.Total) + TaxDeferredInvestmentAccounts.Sum(a => a.Total) +
        TaxFreeInvestmentAccounts.Sum(a => a.Total) + BusinessInterests.Sum(a => a.Total) + Property.Sum(a => a.Total);

    public double TotalLiabilities => Liabilities.Sum(a => a.Total);

    public double YearNetWorth => TotalLiquidAssets + TotalAssets + TotalLiabilities;


    internal YearSummary Clone()
    {
        return new YearSummary() {
            HouseholdIncome = HouseholdIncome,
            CashAccounts = CashAccounts.Select(a => a.Clone()).ToList(),
            AfterTaxInvestmentAccounts = AfterTaxInvestmentAccounts.Select(a => a.Clone()).ToList(),
            TaxDeferredInvestmentAccounts = TaxDeferredInvestmentAccounts.Select(a => a.Clone()).ToList(),
            TaxFreeInvestmentAccounts = TaxFreeInvestmentAccounts.Select(a => a.Clone()).ToList(),
            BusinessInterests = BusinessInterests.Select(a => a.Clone()).ToList(),
            Property = Property.Select(a => a.Clone()).ToList(),
            Liabilities = Liabilities.Select(a => a.Clone()).ToList(),
            DeferredTaxes = DeferredTaxes.Select(a => a.Clone()).ToList(),
        };
    }
}
