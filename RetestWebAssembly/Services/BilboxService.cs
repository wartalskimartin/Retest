using SharedLib.Models;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RetestWebAssembly.Services
{
    public class BilboxService
    {
        private readonly HttpClient HttpClient;
        private readonly string ApiAddressTest = "http://localhost:5047/";
        private readonly string ApiAddress = "https://czvrsrvweb01.intranet.zkw.at:8965/";
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public BilboxService(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
        }

        public async Task SetRetestMode(string bilbox, List<ProductionData> nokList)
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
            return JsonSerializer.Deserialize<List<string>>(content, _jsonSerializerOptions) ?? new List<string>();
        }

        public async Task<List<ProductionData>> GetNokList(string bilbox)
        {
            HttpResponseMessage response = await HttpClient.GetAsync($"{ApiAddress}api/Bilbox/GetActualShiftNOK?standLoc={bilbox}");
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductionData>>(content, _jsonSerializerOptions);
        }

        public async Task UpdateBilboxNokAsync(ProductionData dataItem, IDictionary<string, object> newValues)
        {
            // Change your data here
        }
        public async Task UpdateBilboxNokAsync(ProductionData dataItem, ProductionData newDataItem)
        {
            // Change your data here
        }
    }
}
