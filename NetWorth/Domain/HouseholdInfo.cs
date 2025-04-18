namespace NetWorth.Domain;

public record HouseholdInfo
{
    public string? FirstName { get; set; }
    public string? SpouseFirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? SpouseDateOfBirth { get; set; }

    public int StartTrackingYear => CalculateStartTrackingYear();

    private int CalculateStartTrackingYear()
    {
        int defaultYear = 1940;

        if (DateOfBirth is null && SpouseDateOfBirth is null) {
            return defaultYear;
        }

        return (DateOfBirth ?? SpouseDateOfBirth).Value.Year;
    }
}
