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

        private static Statement BuildDemoStatement()
        {
            static Account J(string type, string name, double balance) =>
                new() { Type = type, Name = name, Balance = balance };
            static Account S(string type, string name, double spouseBalance) =>
                new() { Type = type, Name = name, SpouseBalance = spouseBalance };
            static Account JT(string type, string name, double joint) =>
                new() { Type = type, Name = name, JointBalance = joint };
            static Account Both(string type, string name, double balance, double spouseBalance) =>
                new() { Type = type, Name = name, Balance = balance, SpouseBalance = spouseBalance };

            int y = DateTime.Now.Year; // anchor: current year is the most recent demo year

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
                YearSummaries =
                [
                    new YearSummary
                    {
                        Year = y - 4,
                        HouseholdIncome = 175_000,
                        CashAccounts =
                        [
                            JT("Checking Accounts", "Joint Checking", 15_000),
                            JT("Saving Accounts", "Joint Savings", 25_000),
                            J("Health Savings Account", "John's HSA", 8_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            JT("Brokerage Account #1", "Fidelity Brokerage", 45_000),
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            J("401k - Pre-Tax", "John's Fidelity 401k", 120_000),
                            J("Traditional IRA", "John's Traditional IRA", 35_000),
                            S("401k - Pre-Tax", "Jane's Vanguard 401k", 95_000),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            J("Roth IRA", "John's Roth IRA", 28_000),
                            S("Roth IRA", "Jane's Roth IRA", 22_000),
                        ],
                        Property =
                        [
                            JT("Primary Residence (market value)", "123 Main St", 380_000),
                            J("Automobile #1 (present value)", "John's Truck", 25_000),
                            S("Automobile #2 (present value)", "Jane's SUV", 18_000),
                        ],
                        Liabilities =
                        [
                            JT("Mortgage on Primary Residence", "First National Mortgage", -290_000),
                            J("Auto Loan #1", "John's Truck Loan", -15_000),
                            S("Auto Loan #2", "Jane's SUV Loan", -10_000),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y - 3,
                        HouseholdIncome = 190_000,
                        CashAccounts =
                        [
                            JT("Checking Accounts", "Joint Checking", 18_000),
                            JT("Saving Accounts", "Joint Savings", 32_000),
                            J("Health Savings Account", "John's HSA", 11_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            JT("Brokerage Account #1", "Fidelity Brokerage", 62_000),
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            J("401k - Pre-Tax", "John's Fidelity 401k", 148_000),
                            J("Traditional IRA", "John's Traditional IRA", 42_000),
                            S("401k - Pre-Tax", "Jane's Vanguard 401k", 118_000),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            J("Roth IRA", "John's Roth IRA", 36_000),
                            S("Roth IRA", "Jane's Roth IRA", 28_000),
                        ],
                        Property =
                        [
                            JT("Primary Residence (market value)", "123 Main St", 415_000),
                            J("Automobile #1 (present value)", "John's Truck", 22_000),
                            S("Automobile #2 (present value)", "Jane's SUV", 15_000),
                        ],
                        Liabilities =
                        [
                            JT("Mortgage on Primary Residence", "First National Mortgage", -284_000),
                            J("Auto Loan #1", "John's Truck Loan", -8_000),
                            S("Auto Loan #2", "Jane's SUV Loan", -5_000),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y - 2,
                        HouseholdIncome = 200_000,
                        CashAccounts =
                        [
                            JT("Checking Accounts", "Joint Checking", 20_000),
                            JT("Saving Accounts", "Joint Savings", 38_000),
                            J("Health Savings Account", "John's HSA", 14_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            JT("Brokerage Account #1", "Fidelity Brokerage", 52_000),
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            J("401k - Pre-Tax", "John's Fidelity 401k", 128_000),
                            J("Traditional IRA", "John's Traditional IRA", 36_000),
                            S("401k - Pre-Tax", "Jane's Vanguard 401k", 102_000),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            J("Roth IRA", "John's Roth IRA", 30_000),
                            S("Roth IRA", "Jane's Roth IRA", 23_000),
                        ],
                        Property =
                        [
                            JT("Primary Residence (market value)", "123 Main St", 440_000),
                            J("Automobile #1 (present value)", "John's Truck", 19_000),
                            S("Automobile #2 (present value)", "Jane's SUV", 13_000),
                        ],
                        Liabilities =
                        [
                            JT("Mortgage on Primary Residence", "First National Mortgage", -278_000),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y - 1,
                        HouseholdIncome = 215_000,
                        CashAccounts =
                        [
                            JT("Checking Accounts", "Joint Checking", 22_000),
                            JT("Saving Accounts", "Joint Savings", 45_000),
                            J("Health Savings Account", "John's HSA", 18_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            JT("Brokerage Account #1", "Fidelity Brokerage", 80_000),
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            J("401k - Pre-Tax", "John's Fidelity 401k", 172_000),
                            J("Traditional IRA", "John's Traditional IRA", 50_000),
                            S("401k - Pre-Tax", "Jane's Vanguard 401k", 138_000),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            J("Roth IRA", "John's Roth IRA", 45_000),
                            S("Roth IRA", "Jane's Roth IRA", 35_000),
                        ],
                        Property =
                        [
                            JT("Primary Residence (market value)", "123 Main St", 455_000),
                            J("Automobile #1 (present value)", "John's Truck", 16_000),
                            S("Automobile #2 (present value)", "Jane's SUV", 10_000),
                        ],
                        Liabilities =
                        [
                            JT("Mortgage on Primary Residence", "First National Mortgage", -271_000),
                        ],
                    },
                    new YearSummary
                    {
                        Year = y,
                        HouseholdIncome = 230_000,
                        CashAccounts =
                        [
                            JT("Checking Accounts", "Joint Checking", 26_000),
                            JT("Saving Accounts", "Joint Savings", 55_000),
                            Both("Health Savings Account", "HSA Accounts", 22_000, 8_000),
                        ],
                        AfterTaxInvestmentAccounts =
                        [
                            JT("Brokerage Account #1", "Fidelity Brokerage", 108_000),
                            J("RSUs", "John's Company RSUs", 18_000),
                        ],
                        TaxDeferredInvestmentAccounts =
                        [
                            J("401k - Pre-Tax", "John's Fidelity 401k", 215_000),
                            J("Traditional IRA", "John's Traditional IRA", 63_000),
                            S("401k - Pre-Tax", "Jane's Vanguard 401k", 172_000),
                        ],
                        TaxFreeInvestmentAccounts =
                        [
                            J("401k - Roth", "John's Roth 401k", 28_000),
                            J("Roth IRA", "John's Roth IRA", 58_000),
                            S("Roth IRA", "Jane's Roth IRA", 47_000),
                        ],
                        Property =
                        [
                            JT("Primary Residence (market value)", "123 Main St", 470_000),
                            J("Automobile #1 (present value)", "John's Truck", 13_000),
                            S("Automobile #2 (present value)", "Jane's New SUV", 28_000),
                        ],
                        Liabilities =
                        [
                            JT("Mortgage on Primary Residence", "First National Mortgage", -264_000),
                            S("Auto Loan #2", "Jane's SUV Loan", -24_000),
                        ],
                    },
                ]
            };
        }
    }
}
