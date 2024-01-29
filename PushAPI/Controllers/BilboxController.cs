using Microsoft.AspNetCore.Mvc;
using PushAPI.Services;
using SharedLib.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PushAPI.Controllers
{
    [Route("api/[controller]")]
    public class BilboxController : Controller
    {
        public readonly BibloxService _bilboxService;

        public BilboxController(BibloxService bilboxService)
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
            return Ok(result);
        }

        [HttpPost("addRetestNok")]
        public async Task<IActionResult> AddRetestNok(List<QualityCardBilbox> retestedNok)
        {
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
            List<QualityCardBilbox> result = PopulateModel(pp);

            return Ok(result);
        }
        
        private List<QualityCardBilbox> PopulateModel(List<ProductionData> productionDatas)
        {
            List<QualityCardBilbox> result = new();
            DateTime startOfActualShift = ProductionDataShiftRecognizer.GetActualShiftStartHour();

            foreach (var productionData in productionDatas)
            {
                result.Add( new QualityCardBilbox { 
                    ProductNumber = productionData.ProductNumber,
                    OrderNumber = productionData.OrderNumber,
                    OperationNumber = productionData.Operation,
                    OperatorNumber = productionData.OperatorNumber,
                    WorkingPlaceNumber = productionData.Loc,
                    DeviceNumber = productionData.Device,
                    TeamNumber = productionData.TeamNumber,
                    BilboxNumber = productionData.BbxNum,
                    NokDateTime = productionData.DatetimeServer,
                    IsFromPreviousShift = startOfActualShift > productionData.DatetimeServer
                });
            }

            return result;
        }
    }
}
