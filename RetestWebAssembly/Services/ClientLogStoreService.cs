using Microsoft.JSInterop;
using SharedLib.DTO;

namespace RetestWebAssembly.Services
{
    public class ClientLogStoreService(IJSRuntime js)
    {
        public ValueTask AddAsync(ClientLogEntryDto entry) =>
            js.InvokeVoidAsync("clientLogStore.add", entry);

        public ValueTask<List<ClientLogEntryDto>> GetBatchAsync(int maxCount) =>
            js.InvokeAsync<List<ClientLogEntryDto>>("clientLogStore.getBatch", maxCount);

        public ValueTask DeleteBatchAsync(IEnumerable<long> ids) =>
            js.InvokeVoidAsync("clientLogStore.deleteBatch", ids.ToArray());

        public ValueTask<int> PruneAsync(int maxAgeDays, int maxCount) =>
            js.InvokeAsync<int>("clientLogStore.prune", maxAgeDays, maxCount);
    }
}
