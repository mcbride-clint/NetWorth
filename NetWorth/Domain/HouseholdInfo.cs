namespace NetWorth.Domain;

public record HouseholdInfo
{
    public string? FirstName { get; set; }
    public string? SpouseFirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? SpouseDateOfBirth { get; set; }
}
