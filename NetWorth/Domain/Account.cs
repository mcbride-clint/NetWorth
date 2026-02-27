
namespace NetWorth.Domain;

public class Account
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public double? Balance { get; set; }
    public double? SpouseBalance { get; set; }
    public double? JointBalance { get; set; }
    public double Total => (Balance ?? 0) + (SpouseBalance ?? 0) + (JointBalance ?? 0);

    internal Account Clone()
    {
        return new Account()
        {
            Type = Type,
            Name = Name,
            Balance = Balance,
            SpouseBalance = SpouseBalance,
            JointBalance = JointBalance
        };
    }
}
