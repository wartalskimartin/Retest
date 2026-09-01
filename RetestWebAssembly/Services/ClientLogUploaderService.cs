using SharedLib.DTO;
using System.Net.Http.Json;

namespace RetestWebAssembly.Services
{
    public class ClientLogUploaderService(ClientLogStoreService store, HttpClient httpClient)
    {
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);
        private readonly string _apiAddress = "https://czvrsrvweb01.intranet.zkw.at:8993/";//93 65
        private readonly CancellationTokenSource _cts = new();

        public void Start() => _ = Task.Run(RunAsync);

        private async Task RunAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await FlushOnceAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Log uploader failed: {ex.Message}");
                }

                await Task.Delay(_interval, _cts.Token);
            }
        }

        public async Task FlushOnceAsync()
        {
            await store.PruneAsync(maxAgeDays: 14, maxCount: 30000);

            while (true)
            {
                var batch = await store.GetBatchAsync(200);
                if (batch.Count == 0) return;

                var payload = new ClientLogBatchDto
                {
                    DeviceId = Environment.MachineName,
                    Items = batch
                };

                var response = await httpClient.PostAsJsonAsync($"{_apiAddress}api/clientlogs/batch", payload);
                if (!response.IsSuccessStatusCode) return;

                var ids = batch.Where(x => x.LocalId.HasValue).Select(x => x.LocalId!.Value);
                await store.DeleteBatchAsync(ids);
            }
        }
    }
}
