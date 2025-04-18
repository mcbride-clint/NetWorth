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
                    var statement = JsonSerializer.Deserialize<Statement>(json);
                    if (statement != null)
                    {
                        Current = statement;
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
    }
}
