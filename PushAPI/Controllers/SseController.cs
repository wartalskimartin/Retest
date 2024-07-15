using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PushAPI.Services;
using SharedLib.DTO;
using SharedLib.Models;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace PushAPI.Controllers
{
    [Route("api/[controller]")]
    public class SseController : Controller
    {
        private static readonly string NORMAL_STATE = "Normal";
        private static readonly string RETEST_STATE = "Retest";

        private static ConcurrentDictionary<string, DeviceStateUpdateModel> connectedDevices = new ConcurrentDictionary<string, DeviceStateUpdateModel>();

        public readonly BilboxService _bilboxService;

        public SseController(BilboxService bilboxService)
        {
            _bilboxService = bilboxService;
        }

        /// <summary>
        /// Sends information about number of connceted devices.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("notifyNumberOfDevicesChanged")]
        public async Task NotifyNumberOfBilboxes(CancellationToken cancellationToken)
        {
            // Set the response headers for SSE
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Perform data generation here
                    var eventData = connectedDevices.Count;

                    var eventBytes = Encoding.UTF8.GetBytes($"data: {eventData}\n\n");

                    await Response.Body.WriteAsync(eventBytes, 0, eventBytes.Length);
                    await Response.Body.FlushAsync();

                    await Task.Delay(1000); // Delay between events
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Provides informatin about devices name.
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAvaibleBilboxes")]
        public async Task<List<string>> GetAvailableBilboxes(int hallNumber, string row)
        {
            List<string> stands = await _bilboxService.GetStands(hallNumber, row);
            return stands;
        }

        [HttpGet("getBilboxesByHall")]
        public async Task<List<string>> GetAvailableBilboxes(int hallNumber)
        {
            List<string> stands = await _bilboxService.GetStands(hallNumber);
            return stands;
        }

        [HttpGet("getBilboxesByRow")]
        public async Task<List<string>> GetAvailableBilboxes(string row)
        {
            List<string> stands = await _bilboxService.GetStandsByRows(row);
            return stands;
        }

        [HttpGet("getAllBilboxes")]
        public async Task<List<string>> GetAvailableBilboxes()
        {
            List<string> stands = await _bilboxService.GetStands();
            return stands;
        }

        /// <summary>
        /// This endpoint will establish long-lived connection with client and send periodic updates. Registration for bilboxes here.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("getBilboxState/{bilboxNum}")]
        public async Task GetBilboxState(CancellationToken cancellationToken, string bilboxNum)
        {
            // Set the response headers for SSE
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            // Add the response and state to the connected devices dictionary
            connectedDevices.TryAdd(bilboxNum, new DeviceStateUpdateModel { Response = Response, DeviceState = NORMAL_STATE });

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Send the event data to the specific SSE connection
                    if (connectedDevices.TryGetValue(bilboxNum, out var deviceState))
                    {
                        // Perform data generation here
                        var eventData = JsonSerializer.Serialize(deviceState.ProductionDatasForRetest);
                        var eventBytes = Encoding.UTF8.GetBytes($"data: {eventData}\n\n");

                        await deviceState.Response.Body.WriteAsync(eventBytes, 0, eventBytes.Length);
                        await deviceState.Response.Body.FlushAsync();
                    }

                    await Task.Delay(1000); // Delay between events
                }
            }
            finally
            {
                // Remove the SSE connection from the connected devices dictionary when the connection is closed or canceled
                connectedDevices.TryRemove(bilboxNum, out _);
            }
        }

        [HttpPost("retestMode/{bilboxNum}/{delayedRetest}")]
        public async Task<IActionResult> RetestModeAsync(string bilboxNum, bool delayedRetest, [FromBody] List<QualityCardBilbox> productionDatas)
        {
            List<ProductionDataDTO> productionDataDTOs = PopulateDTO(productionDatas, delayedRetest);

            Console.WriteLine(bilboxNum + RETEST_STATE);
            bool result = await ChangeDeviceState(bilboxNum, RETEST_STATE, productionDataDTOs);

            if(result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost("normalMode/{bilboxNum}")]
        public async Task<IActionResult> NormalModeAsync(string bilboxNum)
        {
            Console.WriteLine(bilboxNum + NORMAL_STATE);
            bool result = await ChangeDeviceState(bilboxNum, NORMAL_STATE);

            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        private async Task<bool> ChangeDeviceState(string bilboxNum, string stateToChange, List<ProductionDataDTO>? productionDatas = null)
        {
            const int maxRetries = 3; // Maximum number of retries
            int delayMilliseconds = 1000; // Initial delay in milliseconds

            for (int currentRetry = 1; currentRetry <= maxRetries; currentRetry++)
            {
                if (connectedDevices.TryGetValue(bilboxNum, out var deviceState))
                {
                    // Perform data generation here
                    deviceState.DeviceState = stateToChange;

                    // Set up NOK production data to be retested
                    if (productionDatas != null && stateToChange == RETEST_STATE)
                    {
                        deviceState.ProductionDatasForRetest = productionDatas;
                    }

                    // Clean up after retest
                    if (stateToChange == NORMAL_STATE)
                    {
                        deviceState.ProductionDatasForRetest = null;
                    }

                    return true; // Exit the method successfully
                }
                else
                {
                    // Log the retry attempt
                    Console.WriteLine($"Attempt {currentRetry}: {bilboxNum} not found. Retrying in {delayMilliseconds}ms...");

                    // Wait before retrying
                    await Task.Delay(delayMilliseconds);

                    // Increment the delay for the next retry
                    delayMilliseconds *= currentRetry + 1;
                }
            }

            // Log or handle the failure to find the bilboxNum after all retries
            Console.WriteLine($"Failed to change device state for {bilboxNum} after {maxRetries} retries.");
            return false;
        }

        private List<ProductionDataDTO> PopulateDTO(List<QualityCardBilbox> productionDatas, bool delayedRetest)
        {
            List<ProductionDataDTO> result = new();

            foreach (var productionData in productionDatas)
            {
                result.Add(new ProductionDataDTO
                {
                    ProductNumber = productionData.ProductNumber,
                    OrderNumber = productionData.OrderNumber,
                    Operation = productionData.OperationNumber,
                    OperatorNumber = productionData.OperatorNumber,
                    Loc = productionData.WorkingPlaceNumber,
                    Device = productionData.DeviceNumber,
                    TeamNumber = productionData.TeamNumber,
                    BbxNum = productionData.BilboxNumber,
                    DatetimeServer = productionData.NokDateTime,
                    DelayedRetest = delayedRetest,
                    Id = productionData.ProductionDataNokId
                });
            }

            return result;
        }
    }

    public class DeviceStateUpdateModel
    {
        public string DeviceState { get; set; }
        public HttpResponse Response { get; set; }
        public List<ProductionDataDTO>? ProductionDatasForRetest { get; set; }
    }
}
