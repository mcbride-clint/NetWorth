using Microsoft.AspNetCore.Components;
using NetWorth.Services;
using MudBlazor;
using System.Threading.Tasks;

public abstract class SaveResetPageBase<T> : ComponentBase
{
    [Inject] protected StatementService StatementService { get; set; }
    [Inject] protected ISnackbar Snackbar { get; set; }

    protected async Task Save() {
        await StatementService.SaveAsync();
        Snackbar.Add("Saved successfully!", Severity.Success);
    }
    protected async Task Reset()
    {
        await StatementService.ResetAsync();
        Snackbar.Add("Reset to last saved state.", Severity.Info);
    }
}
