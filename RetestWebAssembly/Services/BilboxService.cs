using SharedLib.DTO;
using SharedLib.Models;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RetestWebAssembly.Services
{
    public class BilboxService
    {
        private readonly HttpClient HttpClient;
        private readonly string ApiAddress = "http://localhost:5047/";
        private readonly string ApiAddressTest = "https://czvrsrvweb01.intranet.zkw.at:8965/";
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public BilboxService(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
        }

        public async Task SetRetestMode(string bilbox, List<ProductionDataDTO> nokList)
        {
            //var json = JsonSerializer.Serialize(nokList);
            //HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsJsonAsync($"{ApiAddress}api/sse/retestMode/{bilbox}", nokList);

            if (response.IsSuccessStatusCode)
            {
                // Handle a successful response here
            }
            else
            {
                // Handle errors or failed response
            }
        }

        public async Task SetNormalMode(string bilbox)
        {
            await HttpClient.PostAsync($"{ApiAddress}api/sse/normalMode/{bilbox}", null);
        }

        public async Task<List<string>> GetAvaibleBilboxes()
        {
            HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/sse/getAvaibleBilboxes");
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();//change to be same
        }

        public async Task<List<QualityCardBilbox>?> GetNokList(string bilbox)
        {
            HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/GetActualShiftNOK?standLoc={bilbox}");
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<QualityCardBilbox>>(content, _jsonSerializerOptions);
        }

        public async Task UpdateBilboxNokAsync(QualityCardBilbox dataItem, IDictionary<string, object> newValues)
        {
            // Change your data here
        }
        public async Task UpdateBilboxNokAsync(QualityCardBilbox dataItem, QualityCardBilbox newDataItem)
        {
            // Change your data here
        }

        public async Task<List<QualityDefectDescriptionBilbox>?> GetDefectCodes()
        {
            HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/getDefectCodes");
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<QualityDefectDescriptionBilbox>>(content, _jsonSerializerOptions);
        }
    }
}
