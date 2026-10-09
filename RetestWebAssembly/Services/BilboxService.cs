using SharedLib.DTO;
using SharedLib.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace RetestWebAssembly.Services
{   
    public class BilboxService
    {
        private readonly HttpClient HttpClient;
        //private readonly string ApiAddress = "http://localhost:5047/";
        private readonly string ApiAddress = "https://czvrsrvweb01.intranet.zkw.at:8993/";//93 65
        //private readonly string ApiAddressNoSsl = "http://10.13.0.19:8964/";
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(15);
        private readonly TimeSpan _getRetryDelay = TimeSpan.FromMilliseconds(400);
        private const int GetMaxAttempts = 2; // first try + one retry

        public BilboxService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private async Task<T> ExecuteWithTimeoutAsync<T>(Func<CancellationToken, Task<T>> action, T fallbackValue, string operationName)
        {
            using var cts = new CancellationTokenSource(_requestTimeout);
            try
            {
                return await action(cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout error in {operationName}: exceeded {_requestTimeout.TotalSeconds} s.");
                return fallbackValue;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error in {operationName}: {ex.Message}");
                return fallbackValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in {operationName}: {ex.Message}");
                return fallbackValue;
            }
        }

        private async Task ExecuteWithTimeoutAsync(Func<CancellationToken, Task> action, string operationName)
        {
            using var cts = new CancellationTokenSource(_requestTimeout);
            try
            {
                await action(cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout error in {operationName}: exceeded {_requestTimeout.TotalSeconds} s.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error in {operationName}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in {operationName}: {ex.Message}");
            }
        }

        // GET-only wrapper: retry once on timeout/network error
        private async Task<T> ExecuteGetWithTimeoutRetryAsync<T>(Func<CancellationToken, Task<T>> action, T fallbackValue, string operationName)
        {
            for (int attempt = 1; attempt <= GetMaxAttempts; attempt++)
            {
                using var cts = new CancellationTokenSource(_requestTimeout);
                try
                {
                    return await action(cts.Token);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    Console.WriteLine($"Timeout in {operationName} (attempt {attempt}/{GetMaxAttempts}): exceeded {_requestTimeout.TotalSeconds} s.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Network error in {operationName} (attempt {attempt}/{GetMaxAttempts}): {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error in {operationName}: {ex.Message}");
                    return fallbackValue;
                }

                if (attempt < GetMaxAttempts)
                {
                    await Task.Delay(_getRetryDelay);
                }
            }

            return fallbackValue;
        }

        private async Task<List<string>> GetStringListAsync(string requestUri, string operationName)
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync(requestUri, token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<string>>(_jsonSerializerOptions, token)
                    ?? new List<string>();
            }, new List<string>(), operationName);
        }

        public async Task<bool> SetRetestMode(string bilbox, List<QualityCardBilbox> nokList, List<QualityCardBilbox> selectedNok, bool delayedRetest)
        {
            return await ExecuteWithTimeoutAsync(async token =>
            {
                if (selectedNok is null)
                {
                    return false;
                }

                var responseMessage = await HttpClient.PostAsJsonAsync(
                    $"{ApiAddress}api/sse/retestMode/{bilbox}/{delayedRetest}",
                    selectedNok,
                    token);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return false;
                }

                return await SaveQualityCardListInternalAsync(nokList, token);
            }, false, nameof(SetRetestMode));
        }

        public async Task SetNormalMode(string bilbox)
        {
            await ExecuteWithTimeoutAsync(async token =>
            {
                await HttpClient.PostAsync($"{ApiAddress}api/sse/normalMode/{bilbox}", null, token);
            }, nameof(SetNormalMode));
        }

        public async Task<List<string>> GetHalls()
        {
            return await GetStringListAsync($"{ApiAddress}api/bilbox/getHalls", nameof(GetHalls));
        }

        public async Task<List<string>> GetRows()
        {
            return await GetStringListAsync($"{ApiAddress}api/bilbox/getAllRows", nameof(GetRows));
        }

        public async Task<List<string>> GetRows(string hall)
        {
            return await GetStringListAsync($"{ApiAddress}api/bilbox/getRowsByHall?hallNumber={hall}", nameof(GetRows));
        }

        public async Task<List<string>> GetAvaibleBilboxes(string hall, string row)
        {
            return await GetStringListAsync($"{ApiAddress}api/sse/getAvaibleBilboxes?hallNumber={hall}&row={row}", nameof(GetAvaibleBilboxes));
        }

        public async Task<List<string>> GetAvaibleBilboxesByRow(string row)
        {
            return await GetStringListAsync($"{ApiAddress}api/sse/getBilboxesByRow?row={row}", nameof(GetAvaibleBilboxesByRow));
        }

        public async Task<List<string>> GetAvaibleBilboxes(string hall)
        {
            return await GetStringListAsync($"{ApiAddress}api/sse/getBilboxesByHall?hallNumber={hall}", nameof(GetAvaibleBilboxes));
        }

        public async Task<List<string>> GetAvaibleBilboxes()
        {
            return await GetStringListAsync($"{ApiAddress}api/sse/getAllBilboxes", nameof(GetAvaibleBilboxes));
        }

        public async Task<List<QualityCardBilbox>?> GetNokList(string bilbox)
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getActualShiftNOK?standLoc={bilbox}", token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<QualityCardBilbox>>(_jsonSerializerOptions, token)
                    ?? new List<QualityCardBilbox>();
            }, new List<QualityCardBilbox>(), nameof(GetNokList));
        }

        private async Task<bool> SaveQualityCardListInternalAsync(List<QualityCardBilbox> nokList, CancellationToken cancellationToken)
        {
            var response = await HttpClient.PostAsJsonAsync($"{ApiAddress}api/Bilbox/addRetestNok", nokList, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task SaveQualityCardListAsync(List<QualityCardBilbox> nokList)
        {
            await ExecuteWithTimeoutAsync(async token =>
            {
                await SaveQualityCardListInternalAsync(nokList, token);
            }, nameof(SaveQualityCardListAsync));
        }

        public async Task<List<QualityDefectDescriptionBilbox>?> GetDefectCodes()
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getDefectCodes", token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<QualityDefectDescriptionBilbox>>(_jsonSerializerOptions, token)
                    ?? new List<QualityDefectDescriptionBilbox>();
            }, new List<QualityDefectDescriptionBilbox>(), nameof(GetDefectCodes));
        }

        public async Task<StandsList?> GetBilboxDetail(string bilboxName)
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getBilboxInformation/{bilboxName}", token);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(token);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<StandsList>(content, _jsonSerializerOptions);
            }, null, nameof(GetBilboxDetail));
        }

        public async Task<QualityCardBilbox?> GetLastProductionData(string standLoc)
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getLastProductionData?standLoc={standLoc}", token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<QualityCardBilbox>(cancellationToken: token);
            }, null, nameof(GetLastProductionData));
        }

        public async Task<List<QualityCardBilbox>> GetQualityCardBilboxData(string standLoc)
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getQualityCardBilboxData?standLoc={standLoc}", token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<QualityCardBilbox>>(cancellationToken: token) ?? new List<QualityCardBilbox>();
            }, new List<QualityCardBilbox>(), nameof(GetQualityCardBilboxData));
        }

        public async Task InsertQualityCardBilbox(QualityCardBilbox qcb)
        {
            await ExecuteWithTimeoutAsync(async token =>
            {
                var response = await HttpClient.PostAsJsonAsync($"{ApiAddress}api/Bilbox/insertQualityCardBilbox", qcb, token);
                response.EnsureSuccessStatusCode();
            }, nameof(InsertQualityCardBilbox));
        }

        public async Task RemoveQualityCardBilbox(QualityCardBilbox qcb)
        {
            await ExecuteWithTimeoutAsync(async token =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"{ApiAddress}api/Bilbox/removeQualityCardBilbox"),
                    Content = JsonContent.Create(qcb)
                };

                var response = await HttpClient.SendAsync(request, token);
                response.EnsureSuccessStatusCode();
            }, nameof(RemoveQualityCardBilbox));
        }

        public async Task<int> GetStandRetest(string stand)
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getStandsHistory?stand={stand}", token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<int?>(cancellationToken: token) ?? 0;
            }, 0, nameof(GetStandRetest));
        }

        public async Task<List<RetestKomponentVadyDTO>> GetKomponentVady()
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getKomponentVady", token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<RetestKomponentVadyDTO>>(_jsonSerializerOptions, token)
                    ?? new List<RetestKomponentVadyDTO>();
            }, new List<RetestKomponentVadyDTO>(), nameof(GetKomponentVady));
        }

        public async Task<List<string>> GetOperatorNumbersByOrderOperation(int orderNumber, int operationNumber)
        {
            return await GetStringListAsync(
                $"{ApiAddress}api/Bilbox/getOperatorNumbersByOrderOperation?orderNumber={orderNumber}&operationNumber={operationNumber}",
                nameof(GetOperatorNumbersByOrderOperation));
        }

        public async Task<List<int?>> GetOperationPredecessors(int orderNumber, int operationNumber)
        {
            return await ExecuteGetWithTimeoutRetryAsync(async token =>
            {
                var response = await HttpClient.GetAsync(
                    $"{ApiAddress}api/Bilbox/getOperationPredecessors?orderNumber={orderNumber}&operationNumber={operationNumber}",
                    token);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<int?>>(_jsonSerializerOptions, token)
                       ?? new List<int?>();
            }, new List<int?>(), nameof(GetOperationPredecessors));
        }
    }
}
