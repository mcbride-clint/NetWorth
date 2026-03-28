# NetWorth

A personal household net worth tracker built with Blazor WebAssembly. Track your financial picture year over year, plan for FIRE, and visualize your wealth-building progress — all without a backend or account sign-up.

All data is stored locally in your browser's IndexedDB. Nothing leaves your device.

---

## Features

- **Multi-year tracking** — Enter balances for each year and watch your net worth trend over time with year-over-year change indicators
- **Account catalog** — Define accounts once (with owner, type, website, and interest rate) and reuse them across years
- **Primary / Spouse / Joint splits** — Track balances separately for each household member and joint accounts
- **FIRE planning** — Calculate progress toward Regular, Lean, Fat, Coast, and Barista FIRE targets based on your actual spending
- **Financial charts** — Net worth over time (line), asset allocation (donut), tax efficiency (donut), asset composition over time (stacked bar), and YoY growth rates (line)
- **Net worth projection** — Toggle a 10-year projection on the net worth chart using your historical CAGR
- **Offline-first** — No internet required after initial page load; data lives in IndexedDB
- **JSON export / import** — Backup and restore your full statement as a JSON file
- **Additional records** — Track life insurance policies, college savings accounts, and real estate notes in the Footnotes section

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8 / Blazor WebAssembly |
| UI | MudBlazor v6 (Material Design, dark theme) |
| Storage | Browser IndexedDB via JS interop |
| Charts | MudBlazor Charts (line, donut, bar) |
| Backend | None — pure client-side |

---

## Getting Started

```bash
# Clone and run
cd NetWorth
dotnet run
```

Dev server runs at:
- `http://localhost:5285`
- `https://localhost:7207`

**Production build:**

```bash
dotnet publish -c Release -o ./publish
```

The output in `./publish/wwwroot` is a static site that can be hosted on GitHub Pages, Azure Static Web Apps, or any static host.

---

## Pages

### Home Dashboard (`/`)
The main financial overview. Shown after data is entered.

- **Summary cards** — Net Worth, Total Assets, Total Liabilities, Savings Rate (income-based)
- **FIRE Progress** — Shown when Annual Expenses is set; choose a FIRE type and see your progress bar, target, and estimated years to reach it
- **Net Worth Over Time** — Line chart with Liquid Assets, Assets, Liabilities, and Net Worth series; optional projection series
- **Asset Allocation** — Donut chart breaking down assets by class (Cash, After-Tax, Tax-Deferred, Tax-Free, Business, Property)
- **Tax Efficiency** — Donut chart showing the split of investment accounts by tax treatment
- **Asset Composition Over Time** — Stacked bar chart showing how your asset mix has evolved year over year
- **YoY Growth Rate** — Line chart comparing net worth and income growth rates (shown when 3+ years of data exist)

### Year Statements (`/statements`)
Annual balance entry — the primary data input page.

- Select a year from the dropdown (range based on household start year)
- Import account names from a prior year to carry forward your account list
- Enter Household Income and Annual Expenses for the year
- Enter balances across four tabs:
  - **Liquid Assets** — Cash, checking, savings, HSA, CDs
  - **Investments** — After-Tax, Tax-Deferred, and Tax-Free sub-tabs (with Annual Contribution tracking)
  - **Property & Business** — Real estate, vehicles, business interests
  - **Liabilities** — Debts (with interest rate column) and deferred taxes
- YoY delta column shows change vs. the prior year for each account

### Household Info (`/household`)
Basic household metadata used throughout the app.

- Primary and Spouse first names, shared last name
- Dates of birth (used for Coast FIRE retirement age calculations)

### Account Catalog (`/accounts`)
Manage the global list of account definitions.

- Add, edit, and delete account definitions
- Fields: Name, Asset Class, Type (preset or custom), Owner (Primary/Spouse/Joint), Website, Interest Rate, Notes, Active toggle
- Search and filter across all accounts
- Inactive accounts are hidden from year statement entry pickers

### Account Detail (`/accounts/{id}`)
View and edit a single account definition.

- Edit all definition fields inline
- Line chart showing the account's balance history across all years
- Table of yearly balances (Primary, Spouse, Joint, Total) with YoY change column

### Footnotes (`/footnotes`)
Supplemental financial records.

- **Life Insurance** — Policy number, type, owner, insured, death benefit, beneficiary, inception date, contact info
- **College Savings** — Child, institution, owner, beneficiary, and account value
- **Real Estate Details** — Free-form notes on properties (mortgages, improvements, rental income, etc.)

---

## Data Storage

Data is stored in your browser's IndexedDB under the key `NetWorthDB`. It is never sent to a server.

**Export:** Download your full statement as a JSON file via the settings menu.
**Import:** Restore from a previously exported JSON file. Legacy data stored in `localStorage` is automatically migrated to IndexedDB on first load.

---

## Domain Model

```
Statement
├── HouseholdInfo          — Names, dates of birth
├── AccountDefinitions[]   — Global account catalog (cross-year)
├── YearSummaries[]        — One entry per tracked year
│   ├── HouseholdIncome
│   ├── AnnualExpenses
│   ├── CashAccounts[]
│   ├── AfterTaxInvestmentAccounts[]
│   ├── TaxDeferredInvestmentAccounts[]
│   ├── TaxFreeInvestmentAccounts[]
│   ├── BusinessInterests[]
│   ├── Property[]
│   ├── Liabilities[]
│   └── DeferredTaxes[]
└── Footnote
    ├── LifeInsurancePlans[]
    ├── CollegeSavingsAccounts[]
    └── RealEstateDetails
```

Each `Account` (balance entry) links to an `AccountDefinition` via an 8-character ID, inheriting the name, type, and owner. Balances are split into `Balance` (primary), `SpouseBalance`, and `JointBalance` fields.
