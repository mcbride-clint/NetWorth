namespace NetWorth.Domain;

public class ScenarioInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public double NetWorth { get; set; }
    public int? LatestYear { get; set; }
}
