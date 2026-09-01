using SharedLib.DTO;

namespace RetestWebAssembly.Services
{
    public class AppLogService(ClientLogStoreService store)
    {
        public Task InfoAsync(string message, Dictionary<string, string>? tags = null) =>
            WriteAsync("Info", message, null, tags);

        public Task ErrorAsync(string message, Exception ex, Dictionary<string, string>? tags = null) =>
            WriteAsync("Error", message, ex.ToString(), tags);

        private async Task WriteAsync(string level, string message, string? exception, Dictionary<string, string>? tags)
        {
            try
            {
                await store.AddAsync(new ClientLogEntryDto
                {
                    UtcTime = DateTime.UtcNow,
                    Level = level,
                    Message = message,
                    Exception = exception,
                    Tags = tags
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"AppLog write failed: {ex.Message}");
            }
        }
    }
}
