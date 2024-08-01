using Microsoft.AspNetCore.Mvc;
using PushAPI.Services;
using SharedLib.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PushAPI.Controllers
{
    [Route("api/[controller]")]
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

                result.Add(new QualityCardBilbox
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
                });
            }

            return result;
        }
    }
}
