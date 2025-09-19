using SharedLib.DTO;
using SharedLib.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RetestWebAssembly.Services
{
    public class BilboxService
    {
        private readonly HttpClient HttpClient;
        //private readonly string ApiAddress = "http://localhost:5047/";
        private readonly string ApiAddress = "https://czvrsrvweb01.intranet.zkw.at:8993/";//93 65
        //private readonly string ApiAddressNoSsl = "http://10.13.0.19:8964/";
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public BilboxService(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
        }

        public async Task<bool> SetRetestMode(string bilbox, List<QualityCardBilbox> nokList, List<QualityCardBilbox> selectedNok, bool delayedRetest)
        {
            //var json = JsonSerializer.Serialize(nokList);
            //HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage responseMessage = new();

                if(selectedNok is not null)
                {
                    responseMessage = await HttpClient.PostAsJsonAsync($"{ApiAddress}api/sse/retestMode/{bilbox}/{delayedRetest}", selectedNok);
                }

                if(responseMessage.IsSuccessStatusCode)
                {
                    // save rows
                    await SaveQualityCardListAsync(nokList);
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                // Ošetření chyb sítě
                Console.WriteLine($"Network error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Ošetření jiných neočekávaných chyb
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
            return false;
        }

        public async Task SetNormalMode(string bilbox)
        {
            try
            {
                await HttpClient.PostAsync($"{ApiAddress}api/sse/normalMode/{bilbox}", null);
            }
            catch (HttpRequestException ex)
            {
                // Ošetření chyb sítě
                Console.WriteLine($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Ošetření jiných neočekávaných chyb
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<List<string>> GetHalls()
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/bilbox/getHalls");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetRows()
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/bilbox/getAllRows");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetRows(string hall)
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/bilbox/getRows?hallNumber={hall}");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetAvaibleBilboxes(string hall, string row)
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/sse/getAvaibleBilboxes?hallNumber={hall}&row={row}");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetAvaibleBilboxesByRow(string row)
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/sse/getBilboxesByRow?row={row}");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetAvaibleBilboxes(string hall)
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/sse/getBilboxesByHall?hallNumber={hall}");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetAvaibleBilboxes()
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/sse/getAllBilboxes");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<QualityCardBilbox>?> GetNokList(string bilbox)
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getActualShiftNOK?standLoc={bilbox}");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<QualityCardBilbox>>(content, _jsonSerializerOptions);

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<QualityCardBilbox>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<QualityCardBilbox>();
            }
        }

        public async Task SaveQualityCardListAsync(List<QualityCardBilbox> nokList)
        {
            // Change your data here
            var response = await HttpClient.PostAsJsonAsync($"{ApiAddress}api/Bilbox/addRetestNok", nokList);

            if (response.IsSuccessStatusCode)
            {
                // Handle a successful response here
            }
            else
            {
                // Handle errors or failed response
            }
        }

        public async Task<List<QualityDefectDescriptionBilbox>?> GetDefectCodes()
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getDefectCodes");
                string content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<QualityDefectDescriptionBilbox>>(content, _jsonSerializerOptions);

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<QualityDefectDescriptionBilbox>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<QualityDefectDescriptionBilbox>();
            }
        }

        public async Task<StandsList?> GetBilboxDetail(string bilboxName)
        {
            try
            {
                string url = $"{ApiAddress}api/Bilbox/getBilboxInformation/{bilboxName}";
                HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getBilboxInformation/{bilboxName}");
                string content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                {
                    return null;
                }
                return JsonSerializer.Deserialize<StandsList>(content, _jsonSerializerOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }

        /*---------------------------------Manual NOK recording----------------------------------------------------*/

        public async Task<QualityCardBilbox?> GetLastProductionData(string standLoc)
        {
            try
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getLastProductionData?standLoc={standLoc}");
                return await response.Content.ReadFromJsonAsync<QualityCardBilbox>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<QualityCardBilbox>> GetQualityCardBilboxData(string standLoc)
        {
            try
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getQualityCardBilboxData?standLoc={standLoc}");
                return await response.Content.ReadFromJsonAsync<List<QualityCardBilbox>>() ?? new();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<QualityCardBilbox>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new List<QualityCardBilbox>();
            }
        }

        public async Task InsertQualityCardBilbox(QualityCardBilbox qcb)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync($"{ApiAddress}api/Bilbox/insertQualityCardBilbox", qcb);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public async Task RemoveQualityCardBilbox(QualityCardBilbox qcb)
        {
            try
            {
                await HttpClient.SendAsync(new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"{ApiAddress}api/Bilbox/removeQualityCardBilbox"),
                    Content = JsonContent.Create(qcb)
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        /*---------------------------------Show history by stands----------------------------------------------------*/

        public async Task<int> GetStandRetest(string stand)
        {
            try
            {
                var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getStandsHistory?stand={stand}");
                return await response.Content.ReadFromJsonAsync<int>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return 0;
            }
        }

        /*
        public async Task<List<StandsHistoryDTO>> GetStandsHistory()
        {
            var response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getStandsHistory");
            List<StandsHistoryDTO> result = new();
            try
            {
                result = await response.Content.ReadFromJsonAsync<List<StandsHistoryDTO>>() ?? new();
            }
            catch (JsonException jsonEx)
            {
                // Handle JSON parsing errors
                Console.WriteLine($"JSON error: {jsonEx.Message}");
            }
            catch (HttpRequestException httpEx)
            {
                // Handle HTTP request errors
                Console.WriteLine($"HTTP error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return result;
        }*/

    }
}
