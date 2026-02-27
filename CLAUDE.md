# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run dev server (from NetWorth/ directory)
dotnet run

# Build for production
dotnet publish -c Release -o ./publish
```

Dev server runs at `https://localhost:7207` (HTTPS) or `http://localhost:5285` (HTTP).

No npm/build steps — pure .NET build. No test project exists.

## Architecture

**NetWorth** is a Blazor WebAssembly app (.NET 8) for tracking household net worth. All data is stored in browser IndexedDB — no backend/API.

### Key Layers

- **[Domain/](NetWorth/Domain/)** — Core models. `Statement` is the root aggregate containing `HouseholdInfo` and a list of `YearSummary`. Each `YearSummary` holds categorized `Account` lists (cash, investments, property, liabilities, etc.).
- **[Services/StatementService.cs](NetWorth/Services/StatementService.cs)** — Singleton state manager. Maintains `Saved` (persisted) and `Current` (working copy) states. `SaveAsync()` commits to IndexedDB; `ResetAsync()` reverts.
- **[Pages/](NetWorth/Pages/)** — Blazor page components. Pages extending `SaveResetPageBase<T>` get save/reset lifecycle management.
- **[Components/](NetWorth/Components/)** — Reusable UI components (e.g., `AccountList.razor` wraps MudBlazor DataGrid for inline account editing).
- **[wwwroot/js/](NetWorth/wwwroot/js/)** — JS interop for IndexedDB (`statementStorage.js`) and file import/export (`statementFileInterop.js`).

### UI Framework

MudBlazor v6 (Material Design). Uses `MudDataGrid` for inline editing, `MudChart` for visualizations on the Home dashboard.

### Data Persistence

JSON serialized to IndexedDB (`NetWorthDB` database, `statements` object store) via JS interop. File export/import also supported for backup/restore. On first load, any existing `localStorage` data is automatically migrated to IndexedDB.

### Domain Model — Key Fields

**Account** fields: `Type, Name, Balance, SpouseBalance, JointBalance, Notes, AnnualContribution, InterestRate` (fraction, e.g. 0.0675 = 6.75%), `Total` (computed).

**YearSummary** fields: `Year, HouseholdIncome, AnnualExpenses` (new), plus account lists. Computed: `TotalLiquidAssets, TotalAssets, TotalLiabilities, YearNetWorth`.

### AccountList.razor Parameters

`Items`, `Class (AssetClass)`, `OnValueChange`, `PriorYearItems` (enables YoY delta column), `ShowContribution` (shows Annual Contribution column — used for investment tabs), `ShowInterestRate` (shows Interest Rate column — used for Liabilities tab).

### Home Dashboard Sections

1. **Summary Cards** — Net Worth, Total Assets, Liabilities, Savings Rate
2. **FIRE Progress** — shown when `AnnualExpenses > 0`; dropdown selects type (Regular/Lean/Fat/Coast/Barista FIRE); Coast FIRE shows retirement age input
3. **Net Worth Over Time** (line chart) — with "Show Projection" toggle (10-yr CAGR-based projection as 5th series in amber)
4. **Asset Allocation** (donut) + **Tax Efficiency** (donut: After-Tax / Tax-Deferred / Tax-Free split)
5. **Asset Composition Over Time** (stacked bar) — 6 categories per year
6. **YoY Growth Rate** (line chart) — shown when ≥ 3 years; % growth for income and net worth
