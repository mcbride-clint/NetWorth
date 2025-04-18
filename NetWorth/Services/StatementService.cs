using NetWorth.Domain;

namespace NetWorth.Services
{
    public class StatementService
    {
        private Statement Saved { get; set; } = new Statement();
        public Statement Current { get; private set; }

        public StatementService()
        {
            // Initialize the working copy
            Current = Saved.Clone();
        }

        public Task SaveAsync()
        {
            Saved = Current.Clone();
            return Task.CompletedTask;
        }

        public Task ResetAsync()
        {
            Current = Saved.Clone();
            return Task.CompletedTask;
        }
    }
}
