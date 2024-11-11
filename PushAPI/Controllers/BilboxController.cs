using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PushAPI.Services;
using SharedLib.DTO;
using SharedLib.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PushAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificMethods")] // Apply the CORS policy
    public class BilboxController : Controller
    {
        public readonly BilboxService _bilboxService;

        public BilboxController(BilboxService bilboxService)
        {
            _bilboxService = bilboxService;
        }

        [HttpGet("getBilboxInformation/{bilboxName}")]
        public async Task<IActionResult> GetBilboxInformation(string bilboxName)
        {
            StandsList? result = await _bilboxService.GetBilboxInformation(bilboxName);

            if (result == null)
            {
                return NotFound();
            }

            result.QualityCounter = await _bilboxService.GetQualityCounter(bilboxName);

            return Ok(result);
        }

        [HttpPost("addRetestNok")]
        public async Task<IActionResult> AddRetestNok([FromBody] List<QualityCardBilbox> retestedNok)
        {
            retestedNok.ForEach(q => q.DatetimeServer = DateTime.Now);
            await _bilboxService.AddQualityCardBilbox(retestedNok);
            return Ok();
        }

        [HttpGet("getDefectCodes")]
        public async Task<IActionResult> GetDefectCodes()
        {
            List<QualityDefectDescriptionBilbox> result = await _bilboxService.GetDefectCodes();
            return Ok(result);
        }

        [HttpGet("getActualShiftNOK")]
        public async Task<IActionResult> GetActualShiftNOK(string standLoc)
        {
            if (string.IsNullOrEmpty(standLoc))
            {
                return Ok(new List<QualityCardBilbox>());
            }

            DateTime startOfPreviousShift = ProductionDataShiftRecognizer.GetPreviousShiftStartHour();

            List<ProductionData> pp = await _bilboxService.GetLatestNOKFrom(standLoc, startOfPreviousShift);
            int? latestOperatorNumber = await _bilboxService.GetActualOperatorNumber(standLoc);
            List<QualityCardBilbox> result = PopulateModel(pp, latestOperatorNumber);

            return Ok(result);
        }

        [HttpGet("getHalls")]
        public async Task<IActionResult> GetHalls()
        {
            List<string> halls = await _bilboxService.GetHalls();

            return Ok(halls);
        }

        [HttpGet("getAllRows")]
        public async Task<IActionResult> GetRows()
        {
            List<string> rows = await _bilboxService.GetRows();

            return Ok(rows);
        }

        [HttpGet("getRows")]
        public async Task<IActionResult> GetRows(int hallNumber)
        {
            List<string> rows = await _bilboxService.GetRows(hallNumber);

            return Ok(rows);
        }

        private List<QualityCardBilbox> PopulateModel(List<ProductionData> productionDatas, int? currentShiftOperatorNumber)
        {
            List<QualityCardBilbox> result = new();
            DateTime startOfActualShift = ProductionDataShiftRecognizer.GetActualShiftStartHour();

            foreach (var productionData in productionDatas)
            {
                bool isFromPreviousShift = startOfActualShift > productionData.DatetimeServer;

                // If the OperatorNumber from the previous shift matches the current shift, set the flag to false
                if (isFromPreviousShift && productionData.OperatorNumber == currentShiftOperatorNumber)
                {
                    isFromPreviousShift = false;
                }

                var qualityCardBilbox = PopulateModelItem(productionData, isFromPreviousShift);
                result.Add(qualityCardBilbox);
            }

            return result;
        }

        public QualityCardBilbox PopulateModelItem(ProductionData productionData, bool isFromPreviousShift)
        {
            return new QualityCardBilbox
            {
                ProductNumber = productionData.ProductNumber,
                OrderNumber = productionData.OrderNumber,
                OperationNumber = productionData.Operation,
                OperatorNumber = productionData.OperatorNumber,
                WorkingPlaceNumber = productionData.Loc,
                DeviceNumber = productionData.Device,
                TeamNumber = productionData.TeamNumber,
                BilboxNumber = productionData.BbxNum,
                NokDateTime = productionData.DatetimeServer,
                IsFromPreviousShift = isFromPreviousShift,
                ProductionDataNokId = productionData.Id
            };
        }

        /*---------------------------------Manual NOK recording----------------------------------------------------*/

        [HttpGet("getLastProductionData")]
        public async Task<ActionResult<QualityCardBilbox>> GetLastProductionData(string standLoc)
        {
            var result = await _bilboxService.GetLastProductionData(standLoc);
            return Ok(PopulateModelItem(result, false));
        }

        [HttpGet("getQualityCardBilboxData")]
        public async Task<ActionResult<List<QualityCardBilbox>>> GetQualityCardBilboxData(string standLoc)
        {
            DateTime startOfPreviousShift = ProductionDataShiftRecognizer.GetPreviousShiftStartHour();
            var result = await _bilboxService.GetQualityCardBilboxData(standLoc, startOfPreviousShift);
            return Ok(result);
        }

        [HttpPost("insertQualityCardBilbox")]
        public async Task<ActionResult> InsertQualityCardBilbox([FromBody] QualityCardBilbox qcb)
        {
            await _bilboxService.InsertQualityCardBilbox(qcb);
            return Ok();
        }

        [HttpDelete("removeQualityCardBilbox")]
        public async Task<ActionResult> RemoveQualityCardBilbox([FromBody] QualityCardBilbox qcb)
        {
            await _bilboxService.RemoveQualityCardBilbox(qcb);
            return Ok();
        }


        /// <summary>
        /// Show history by stands
        /// </summary>
        /// <returns></returns>
        [HttpGet("getStandsHistory")]
        public async Task<ActionResult<List<StandsHistoryDTO>>> GetStandsHistory(string stand)
        {
            DateTime startShift = ProductionDataShiftRecognizer.GetActualShiftStartHour();
            var result = await _bilboxService.GetStandsHistory(startShift, stand);
            return Ok(result);
        }
    }
}
