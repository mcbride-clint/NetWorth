namespace NetWorth.Domain;

public class AssetClass
{
    public AssetClass(string category, string[] presets)
    {
        Category = category;
        Presets = presets;
    }

    public string Category { get; init; }
    public string[] Presets { get; init; }


    public static readonly AssetClass Cash = new("Cash", [
                "Checking Accounts",
                "Saving Accounts",
                "CDs (Certificates of Deposit)",
                "Cash",
                "Life Insurance (Cash Surrender Value)",
                "Health Savings Account",
                "Other"
            ]);

    public static readonly AssetClass AfterTax = new("After-Tax Investment", [
            "Brokerage Account #1",
        "Brokerage Account #2",
        "RSUs",
        "ESPP",
        "Options",
        "Other"
        ]);

    public static readonly AssetClass TaxDeferred = new("Tax-Deferred Investment", [
        "401k - Pre-Tax",
        "403b - Pre-Tax",
        "457 - Pre-Tax",
        "SEP IRA",
        "SIMPLE IRA",
        "Traditional IRA",
        "Rollover IRA",
        "Deferred Compensation",
        "Other"
    ]);

    public static readonly AssetClass TaxFree = new("Tax-Free Investment", [
    "401k - Roth",
        "403b - Roth",
        "457 - Roth",
        "Roth IRA",
        "Health Savings Account",
        "Other"
]);

    public static readonly AssetClass BusinessInterests = new("Business Interests", []);

    public static readonly AssetClass Property = new("Property", [
    "Primary Residence (market value)",
    "Secondary Residence (market value)",
        "Automobile #1 (present value)",
        "Automobile #2 (present value)",
        "Bullion(silver/gold/etc)",
        "Jewelery",
        "Art/Collectibles",
        "Home Furnishings",
        "Boat",
        "Other"
]);

    public static readonly AssetClass Liability = new("Liabilities", [
"Accounts Payable",
        "Auto Loan #1",
        "Auto Loan #2",
        "Credit Card Debt",
        "Consumer Loans or Installments",
        "Loan on Life Insurance",
        "Mortgage on Primary Residence",
        "Mortgage on Secondary Residence",
        "HELOC",
        "Student Loans",
        "Unpaid Taxes",
        "Money Owed to Others",
        "Other Liabilities"
]);
}
