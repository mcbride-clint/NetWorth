
namespace NetWorth.Domain;

public class Statement
{
    public Footnote Footnote { get; set; } = new();
    public HouseholdInfo Household { get; set; } = new();
    public List<AccountDefinition> AccountDefinitions { get; set; } = [];
    public List<YearSummary> YearSummaries { get; set; } = [];

    public IEnumerable<double> YearlyLiquidAssets => YearSummaries.OrderBy(s => s.Year).Select(s => s.TotalLiquidAssets);
    public IEnumerable<double> YearlyAssets => YearSummaries.OrderBy(s => s.Year).Select(s => s.TotalAssets);
    public IEnumerable<double> YearlyLiabilities => YearSummaries.OrderBy(s => s.Year).Select(s => s.TotalLiabilities);
    public IEnumerable<double> YearlyNetWorth => YearSummaries.OrderBy(s => s.Year).Select(s => s.YearNetWorth);

    internal Statement Clone()
    {
        return new Statement()
        {
            Household = Household with { },
            AccountDefinitions = AccountDefinitions.Select(d => d.Clone()).ToList(),
            YearSummaries = YearSummaries.Select(s => s.Clone()).ToList()
        };
    }

    internal IEnumerable<int> ExistingYears => YearSummaries.Where(s => s.Year.HasValue).Select(s => s.Year.Value);

    internal YearSummary GetSummary(int year)
    {
        var summary = YearSummaries.FirstOrDefault(s => s.Year == year);
        if (summary == null)
        {
            summary = new YearSummary() { Year = year };
            YearSummaries.Add(summary);
        }
        return summary;
    }

    internal void SaveSummary(YearSummary summary)
    {
        if (!summary.Year.HasValue)
            throw new InvalidOperationException("YearSummary.Year must be set before saving.");
        YearSummaries.RemoveAll(s => s.Year == summary.Year);
        YearSummaries.Add(summary);
    }
}
