using NetWorth.Domain;
using System.Text.Json;
using Microsoft.JSInterop;

namespace NetWorth.Services
{
    public class StatementService
    {
        private const string StorageKey = "networth_statement";
        private readonly IJSRuntime _jsRuntime;
        private Statement Saved { get; set; } = new Statement();
        public Statement Current { get; set; }
        public bool HasUnsavedChanges { get; private set; }

        public event Action? StateChanged;

        public void MarkDirty()
        {
            HasUnsavedChanges = true;
            StateChanged?.Invoke();
        }

        public StatementService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            // Initialize the working copy
            Current = Saved.Clone();
        }

        public async Task SaveAsync()
        {
            Saved = Current.Clone();
            HasUnsavedChanges = false;
            await SaveToStorageAsync();
            StateChanged?.Invoke();
        }

        public async Task ResetAsync()
        {
            await LoadFromStorageAsync();
            Current = Saved.Clone();
            HasUnsavedChanges = false;
            StateChanged?.Invoke();
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            WriteIndented = false
        };

        public async Task SaveToStorageAsync()
        {
            var statementJson = JsonSerializer.Serialize(Current, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("statementStorage.saveStatement", StorageKey, statementJson);
        }

        public async Task LoadFromStorageAsync()
        {
            var statementJson = await _jsRuntime.InvokeAsync<string>("statementStorage.loadStatement", StorageKey);
            if (!string.IsNullOrEmpty(statementJson))
            {
                try
                {
                    Saved = JsonSerializer.Deserialize<Statement>(statementJson, _jsonOptions) ?? new Statement();
                    MigrateDefinitions(Saved);
                }
                catch
                {
                    Saved = new Statement();
                }
            }
        }

        public async Task ClearStorageAsync()
        {
            await _jsRuntime.InvokeVoidAsync("statementStorage.clearStatement", StorageKey);
        }

        public async Task ExportStatementToFileAsync()
        {
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            await _jsRuntime.InvokeVoidAsync("statementFileInterop.exportStatement", json, $"NetWorthStatement_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        public async Task<bool> ImportStatementFromFileAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("statementFileInterop.importStatement");
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var statement = JsonSerializer.Deserialize<Statement>(json, _jsonOptions);
                    if (statement != null)
                    {
                        MigrateDefinitions(statement);
                        Current = statement;
                        MarkDirty();
                        return true;
                    }
                }
            }
            catch
            {
                // Optionally handle/log error
            }
            return false;
        }

        /// <summary>
        /// Loads a realistic 5-year demo statement into Current without persisting to IndexedDB.
        /// </summary>
        public void LoadDemo()
        {
            Current = BuildDemoStatement();
            MarkDirty();
        }

        /// <summary>
        /// Auto-creates AccountDefinitions from existing account data when loading pre-definition statements.
        /// Safe to call on statements that already have definitions — it returns immediately.
        /// </summary>
        private static void MigrateDefinitions(Statement s)
        {
            if (s.AccountDefinitions.Count > 0) return;

            var allAccountLists = s.YearSummaries.SelectMany(y => new[]
            {
                (Accounts: y.CashAccounts,                   Category: AssetClass.Cash.Category),
                (Accounts: y.AfterTaxInvestmentAccounts,     Category: AssetClass.AfterTax.Category),
                (Accounts: y.TaxDeferredInvestmentAccounts,  Category: AssetClass.TaxDeferred.Category),
                (Accounts: y.TaxFreeInvestmentAccounts,      Category: AssetClass.TaxFree.Category),
                (Accounts: y.BusinessInterests,              Category: AssetClass.BusinessInterests.Category),
                (Accounts: y.Property,                       Category: AssetClass.Property.Category),
                (Accounts: y.Liabilities,                    Category: AssetClass.Liability.Category),
                (Accounts: y.DeferredTaxes,                  Category: AssetClass.DeferredTax.Category),
            });

            // key = "Category|Name" → definition id
            var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (accounts, category) in allAccountLists)
            {
                foreach (var acct in accounts)
                {
                    if (!string.IsNullOrWhiteSpace(acct.DefinitionId)) continue;
                    if (string.IsNullOrWhiteSpace(acct.Name)) continue;

                    var key = $"{category}|{acct.Name}";
                    if (!seen.TryGetValue(key, out var id))
                    {
                        id = Guid.NewGuid().ToString("N")[..8];
                        seen[key] = id;
                        s.AccountDefinitions.Add(new AccountDefinition
                        {
                            Id = id,
                            Name = acct.Name!,
                            AssetClassCategory = category,
                            Type = acct.Type ?? "",
                            Owner = InferOwner(acct),
                        });
                    }
                    acct.DefinitionId = id;
                }
            }
        }

        private static string InferOwner(Account a)
        {
            if (a.Balance.HasValue && a.Balance != 0) return "Primary";
            if (a.SpouseBalance.HasValue && a.SpouseBalance != 0) return "Spouse";
            return "Joint";
        }

        private static Statement BuildDemoStatement()
        {
            // ── Definition IDs ──────────────────────────────────────────────────
            const string dJointChecking   = "d0000001";
            const string dJointSavings    = "d0000002";
            const string dHsa             = "d0000003";
            const string dFidelityBrok    = "d0000004";
            const string dRsus            = "d0000005";
            const string dJohn401k        = "d0000006";
            const string dJohnTradIra     = "d0000007";
            const string dJaneVanguard401k= "d0000008";
            const string dJohnRoth401k    = "d0000009";
            const string dJohnRothIra     = "d0000010";
            const string dJaneRothIra     = "d0000011";
            const string dMainSt          = "d0000012";
            const string dJohnTruck       = "d0000013";
            const string dJaneSuv         = "d0000014";
            const string dMortgage        = "d0000015";
            const string dJohnTruckLoan   = "d0000016";
            const string dJaneSuvLoan     = "d0000017";

            // ── Account helpers ─────────────────────────────────────────────────
            static Account J(string defId, string type, string name, double balance) =>
                new() { DefinitionId = defId, Type = type, Name = name, Balance = balance };
            static Account S(string defId, string type, string name, double spouseBalance) =>
                new() { DefinitionId = defId, Type = type, Name = name, SpouseBalance = spouseBalance };
            static Account JT(string defId, string type, string name, double joint) =>
                new() { DefinitionId = defId, Type = type, Name = name, JointBalance = joint };
            static Account JC(string defId, string type, string name, double balance, double contrib) =>
                new() { DefinitionId = defId, Type = type, Name = name, Balance = balance, AnnualContribution = contrib };
            static Account SC(string defId, string type, string name, double spouseBalance, double contrib) =>
                new() { DefinitionId = defId, Type = type, Name = name, SpouseBalance = spouseBalance, AnnualContribution = contrib };
            static Account BothC(string defId, string type, string name, double balance, double spouseBalance, double contrib) =>
                new() { DefinitionId = defId, Type = type, Name = name, Balance = balance, SpouseBalance = spouseBalance, AnnualContribution = contrib };
            static Account JTL(string defId, string type, string name, double joint, double rate, string? notes = null) =>
                new() { DefinitionId = defId, Type = type, Name = name, JointBalance = joint, InterestRate = rate, Notes = notes };
            static Account JL(string defId, string type, string name, double balance, double rate) =>
                new() { DefinitionId = defId, Type = type, Name = name, Balance = balance, InterestRate = rate };
            static Account SL(string defId, string type, string name, double spouseBalance, double rate) =>
                new() { DefinitionId = defId, Type = type, Name = name, SpouseBalance = spouseBalance, InterestRate = rate };

            int y = DateTime.Now.Year;

            return new Statement
            {
                Household = new HouseholdInfo
                {
                    FirstName = "John",
                    SpouseFirstName = "Jane",
                    LastName = "Smith",
                    DateOfBirth = new DateTime(1980, 4, 12),
                    SpouseDateOfBirth = new DateTime(1982, 9, 5),
                },

                AccountDefinitions =
                [
                    // Cash
                    new() { Id = dJointChecking,    Name = "Joint Checking",            AssetClassCategory = AssetClass.Cash.Category,        Type = "Checking Accounts",                   Owner = "Joint" },
                    new() { Id = dJointSavings,     Name = "Joint Savings",             AssetClassCategory = AssetClass.Cash.Category,        Type = "Saving Accounts",                     Owner = "Joint" },
                    new() { Id = dHsa,              Name = "HSA Accounts",              AssetClassCategory = AssetClass.Cash.Category,        Type = "Health Savings Account",              Owner = "Joint",    Notes = "John's HSA at Fidelity; Jane's HSA at Optum" },
                    // After-Tax
                    new() { Id = dFidelityBrok,     Name = "Fidelity Brokerage",        AssetClassCategory = AssetClass.AfterTax.Category,    Type = "Brokerage Account #1",                Owner = "Joint",    Website = "https://www.fidelity.com", Notes = "Index funds, auto-rebalanced quarterly" },
                    new() { Id = dRsus,             Name = "John's Company RSUs",       AssetClassCategory = AssetClass.AfterTax.Category,    Type = "RSUs",                                Owner = "Primary",  Notes = "Vesting schedule: 25%/yr, 4-yr cliff" },
                    // Tax-Deferred
                    new() { Id = dJohn401k,         Name = "John's Fidelity 401k",      AssetClassCategory = AssetClass.TaxDeferred.Category, Type = "401k - Pre-Tax",                      Owner = "Primary",  Website = "https://www.fidelity.com" },
                    new() { Id = dJohnTradIra,      Name = "John's Traditional IRA",    AssetClassCategory = AssetClass.TaxDeferred.Category, Type = "Traditional IRA",                     Owner = "Primary",  Website = "https://www.fidelity.com" },
                    new() { Id = dJaneVanguard401k, Name = "Jane's Vanguard 401k",      AssetClassCategory = AssetClass.TaxDeferred.Category, Type = "401k - Pre-Tax",                      Owner = "Spouse",   Website = "https://investor.vanguard.com" },
                    // Tax-Free
                    new() { Id = dJohnRoth401k,     Name = "John's Roth 401k",          AssetClassCategory = AssetClass.TaxFree.Category,     Type = "401k - Roth",                         Owner = "Primary",  Website = "https://www.fidelity.com" },
                    new() { Id = dJohnRothIra,      Name = "John's Roth IRA",           AssetClassCategory = AssetClass.TaxFree.Category,     Type = "Roth IRA",                            Owner = "Primary",  Website = "https://www.fidelity.com" },
                    new() { Id = dJaneRothIra,      Name = "Jane's Roth IRA",           AssetClassCategory = AssetClass.TaxFree.Category,     Type = "Roth IRA",                            Owner = "Spouse",   Website = "https://investor.vanguard.com" },
                    // Property
                    new() { Id = dMainSt,           Name = "123 Main St",               AssetClassCategory = AssetClass.Property.Category,    Type = "Primary Residence (market value)",    Owner = "Joint",    Notes = "3 bed / 2 bath, purchased 2018" },
                    new() { Id = dJohnTruck,        Name = "John's Truck",              AssetClassCategory = AssetClass.Property.Category,    Type = "Automobile #1 (present value)",       Owner = "Primary" },
                    new() { Id = dJaneSuv,          Name = "Jane's SUV",                AssetClassCategory = AssetClass.Property.Category,    Type = "Automobile #2 (present value)",       Owner = "Spouse" },
                    // Liabilities
                    new() { Id = dMortgage,         Name = "First National Mortgage",   AssetClassCategory = AssetClass.Liability.Category,   Type = "Mortgage on Primary Residence",       Owner = "Joint",    Website = "https://www.firstnational.com", Notes = "30-yr fixed, refi'd 2021" },
                    new() { Id = dJohnTruckLoan,    Name = "John's Truck Loan",         AssetClassCategory = AssetClass.Liability.Category,   Type = "Auto Loan #1",                        Owner = "Primary" },
                    new() { Id = dJaneSuvLoan,      Name = "Jane's SUV Loan",           AssetClassCategory = AssetClass.Liability.Category,   Type = "Auto Loan #2",                        Owner = "Spouse" },
                ],

                YearSummaries =
                [
                    new YearSummary
                    {
                        Year = y - 4,
                        HouseholdIncome = 175_000,
                        AnnualExpenses = 122_500,
                        CashAccounts =
                        [
                            JT(dJointChecking, "Checking Accounts",    "Joint Checking", 15_000),
                            JT(dJointSavings,  "Saving Accounts",      "Joint Savings",  25_000),
                            J (dHsa,           "Health Savings Account","John's HSA",      8_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            new() { DefinitionId = dFidelityBrok, Type = "Brokerage Account #1", Name = "Fidelity Brokerage", JointBalance = 45_000, Notes = "Index funds, auto-rebalanced quarterly" },
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            JC(dJohn401k,         "401k - Pre-Tax",  "John's Fidelity 401k",   120_000, 19_500),
                            JC(dJohnTradIra,      "Traditional IRA", "John's Traditional IRA",   35_000,  6_000),
                            SC(dJaneVanguard401k, "401k - Pre-Tax",  "Jane's Vanguard 401k",    95_000, 19_500),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            JC(dJohnRothIra, "Roth IRA", "John's Roth IRA", 28_000, 6_000),
                            SC(dJaneRothIra, "Roth IRA", "Jane's Roth IRA", 22_000, 6_000),
                        ],
                        Property =
                        [
                            JT(dMainSt,    "Primary Residence (market value)", "123 Main St",   380_000),
                            J (dJohnTruck, "Automobile #1 (present value)",    "John's Truck",   25_000),
                            S (dJaneSuv,   "Automobile #2 (present value)",    "Jane's SUV",     18_000),
                        ],
                        Liabilities =
                        [
                            JTL(dMortgage,      "Mortgage on Primary Residence", "First National Mortgage", -290_000, 0.0675, "30-yr fixed, refi'd 2021"),
                            JL (dJohnTruckLoan, "Auto Loan #1",                  "John's Truck Loan",        -15_000, 0.0389),
                            SL (dJaneSuvLoan,   "Auto Loan #2",                  "Jane's SUV Loan",          -10_000, 0.0425),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y - 3,
                        HouseholdIncome = 190_000,
                        AnnualExpenses = 133_000,
                        CashAccounts =
                        [
                            JT(dJointChecking, "Checking Accounts",    "Joint Checking", 18_000),
                            JT(dJointSavings,  "Saving Accounts",      "Joint Savings",  32_000),
                            J (dHsa,           "Health Savings Account","John's HSA",     11_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            new() { DefinitionId = dFidelityBrok, Type = "Brokerage Account #1", Name = "Fidelity Brokerage", JointBalance = 62_000, Notes = "Index funds, auto-rebalanced quarterly" },
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            JC(dJohn401k,         "401k - Pre-Tax",  "John's Fidelity 401k",  148_000, 20_500),
                            JC(dJohnTradIra,      "Traditional IRA", "John's Traditional IRA",  42_000,  6_500),
                            SC(dJaneVanguard401k, "401k - Pre-Tax",  "Jane's Vanguard 401k",   118_000, 20_500),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            JC(dJohnRothIra, "Roth IRA", "John's Roth IRA", 36_000, 6_500),
                            SC(dJaneRothIra, "Roth IRA", "Jane's Roth IRA", 28_000, 6_500),
                        ],
                        Property =
                        [
                            JT(dMainSt,    "Primary Residence (market value)", "123 Main St",  415_000),
                            J (dJohnTruck, "Automobile #1 (present value)",    "John's Truck",  22_000),
                            S (dJaneSuv,   "Automobile #2 (present value)",    "Jane's SUV",    15_000),
                        ],
                        Liabilities =
                        [
                            JTL(dMortgage,      "Mortgage on Primary Residence", "First National Mortgage", -284_000, 0.0675, "30-yr fixed, refi'd 2021"),
                            JL (dJohnTruckLoan, "Auto Loan #1",                  "John's Truck Loan",          -8_000, 0.0389),
                            SL (dJaneSuvLoan,   "Auto Loan #2",                  "Jane's SUV Loan",            -5_000, 0.0425),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y - 2,
                        HouseholdIncome = 200_000,
                        AnnualExpenses = 140_000,
                        CashAccounts =
                        [
                            JT(dJointChecking, "Checking Accounts",    "Joint Checking", 20_000),
                            JT(dJointSavings,  "Saving Accounts",      "Joint Savings",  38_000),
                            J (dHsa,           "Health Savings Account","John's HSA",     14_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            new() { DefinitionId = dFidelityBrok, Type = "Brokerage Account #1", Name = "Fidelity Brokerage", JointBalance = 52_000, Notes = "Index funds, auto-rebalanced quarterly" },
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            JC(dJohn401k,         "401k - Pre-Tax",  "John's Fidelity 401k",  128_000, 20_500),
                            JC(dJohnTradIra,      "Traditional IRA", "John's Traditional IRA",  36_000,  6_500),
                            SC(dJaneVanguard401k, "401k - Pre-Tax",  "Jane's Vanguard 401k",   102_000, 20_500),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            JC(dJohnRothIra, "Roth IRA", "John's Roth IRA", 30_000, 6_500),
                            SC(dJaneRothIra, "Roth IRA", "Jane's Roth IRA", 23_000, 6_500),
                        ],
                        Property =
                        [
                            JT(dMainSt,    "Primary Residence (market value)", "123 Main St",  440_000),
                            J (dJohnTruck, "Automobile #1 (present value)",    "John's Truck",  19_000),
                            S (dJaneSuv,   "Automobile #2 (present value)",    "Jane's SUV",    13_000),
                        ],
                        Liabilities =
                        [
                            JTL(dMortgage, "Mortgage on Primary Residence", "First National Mortgage", -278_000, 0.0675, "30-yr fixed, refi'd 2021"),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y - 1,
                        HouseholdIncome = 215_000,
                        AnnualExpenses = 150_500,
                        CashAccounts =
                        [
                            JT(dJointChecking, "Checking Accounts",    "Joint Checking", 22_000),
                            JT(dJointSavings,  "Saving Accounts",      "Joint Savings",  45_000),
                            J (dHsa,           "Health Savings Account","John's HSA",     18_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            new() { DefinitionId = dFidelityBrok, Type = "Brokerage Account #1", Name = "Fidelity Brokerage", JointBalance = 80_000, Notes = "Index funds, auto-rebalanced quarterly" },
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            JC(dJohn401k,         "401k - Pre-Tax",  "John's Fidelity 401k",  172_000, 22_500),
                            JC(dJohnTradIra,      "Traditional IRA", "John's Traditional IRA",  50_000,  7_000),
                            SC(dJaneVanguard401k, "401k - Pre-Tax",  "Jane's Vanguard 401k",   138_000, 22_500),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            JC(dJohnRothIra, "Roth IRA", "John's Roth IRA", 45_000, 7_000),
                            SC(dJaneRothIra, "Roth IRA", "Jane's Roth IRA", 35_000, 7_000),
                        ],
                        Property =
                        [
                            JT(dMainSt,    "Primary Residence (market value)", "123 Main St",  455_000),
                            J (dJohnTruck, "Automobile #1 (present value)",    "John's Truck",  16_000),
                            S (dJaneSuv,   "Automobile #2 (present value)",    "Jane's SUV",    10_000),
                        ],
                        Liabilities =
                        [
                            JTL(dMortgage, "Mortgage on Primary Residence", "First National Mortgage", -271_000, 0.0675, "30-yr fixed, refi'd 2021"),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y,
                        HouseholdIncome = 230_000,
                        AnnualExpenses = 161_000,
                        CashAccounts =
                        [
                            JT(dJointChecking, "Checking Accounts",    "Joint Checking",  26_000),
                            JT(dJointSavings,  "Saving Accounts",      "Joint Savings",   55_000),
                            BothC(dHsa,        "Health Savings Account","HSA Accounts",    22_000, 8_000, 8_300),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            new() { DefinitionId = dFidelityBrok, Type = "Brokerage Account #1", Name = "Fidelity Brokerage", JointBalance = 108_000, Notes = "Index funds, auto-rebalanced quarterly" },
                            new() { DefinitionId = dRsus,         Type = "RSUs",                 Name = "John's Company RSUs", Balance = 18_000, Notes = "Vesting schedule: 25%/yr, 4-yr cliff" },
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            JC(dJohn401k,         "401k - Pre-Tax",  "John's Fidelity 401k",  215_000, 23_000),
                            JC(dJohnTradIra,      "Traditional IRA", "John's Traditional IRA",  63_000,  7_000),
                            SC(dJaneVanguard401k, "401k - Pre-Tax",  "Jane's Vanguard 401k",   172_000, 23_000),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            JC(dJohnRoth401k, "401k - Roth", "John's Roth 401k",  28_000, 5_000),
                            JC(dJohnRothIra,  "Roth IRA",    "John's Roth IRA",   58_000, 7_000),
                            SC(dJaneRothIra,  "Roth IRA",    "Jane's Roth IRA",   47_000, 7_000),
                        ],
                        Property =
                        [
                            JT(dMainSt,    "Primary Residence (market value)", "123 Main St",      470_000),
                            J (dJohnTruck, "Automobile #1 (present value)",    "John's Truck",      13_000),
                            S (dJaneSuv,   "Automobile #2 (present value)",    "Jane's New SUV",    28_000),
                        ],
                        Liabilities =
                        [
                            JTL(dMortgage,    "Mortgage on Primary Residence", "First National Mortgage", -264_000, 0.0675, "30-yr fixed, refi'd 2021"),
                            SL (dJaneSuvLoan, "Auto Loan #2",                  "Jane's SUV Loan",           -24_000, 0.0499),
                        ],
                    },
                ]
            };
        }
    }
}
