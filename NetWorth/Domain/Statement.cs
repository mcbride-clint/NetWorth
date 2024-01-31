
namespace NetWorth.Domain;

public class Statement
{
    public HouseholdInfo Household { get; set; } = new();
    public List<YearSummary> YearSummaries { get; set; } = [];

    internal Statement Clone()
    {
        return new Statement()
        {
            Household = Household with { },
            YearSummaries = YearSummaries.Select(s => s.Clone()).ToList()
        };
    }

    internal IEnumerable<int> ExistingYears => YearSummaries.Where( s => s.Year.HasValue).Select(s => s.Year.Value);

    internal YearSummary GetSummary(int year) => YearSummaries.FirstOrDefault(s => s.Year == year, new());

    internal void SaveSummary(YearSummary summary)
    {
        YearSummaries.RemoveAll(s => s.Year == summary.Year);
        YearSummaries.Add(summary);
    }
}
