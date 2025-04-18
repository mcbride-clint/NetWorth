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
        public Statement Current { get; private set; }

        public StatementService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            // Initialize the working copy
            Current = Saved.Clone();
        }

        public async Task SaveAsync()
        {
            Saved = Current.Clone();
            await SaveToStorageAsync();
        }

        public async Task ResetAsync()
        {
            await LoadFromStorageAsync();
            Current = Saved.Clone();
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
                    Saved    = JsonSerializer.Deserialize<Statement>(statementJson, _jsonOptions) ?? new Statement();
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
    }
}
