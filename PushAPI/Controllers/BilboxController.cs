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
            DateTime startOfShift = GetActualShiftStartHour();

            List<ProductionData> pp = await _bilboxService.GetLatestShiftNOK(standLoc, startOfShift);
            List<QualityCardBilbox> result = PopulateModel(pp);

            return Ok(result);
        }
        
        private List<QualityCardBilbox> PopulateModel(List<ProductionData> productionDatas)
        {
            List<QualityCardBilbox> result = new();

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
                    NokDateTime = productionData.DatetimeServer
                });
            }

            return result;
        }
        
        private DateTime GetActualShiftStartHour()
        {
            DateTime result;
            int hourStartActualShift;

            if(6 <= DateTime.Now.Hour && DateTime.Now.Hour < 14)
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
            }
            else if(14 <= DateTime.Now.Hour && DateTime.Now.Hour < 22) 
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 0, 0);
            }
            else if (22 <= DateTime.Now.Hour && DateTime.Now.Hour <= 23)
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 22, 0, 0);
            }
            else //if (0 <= DateTime.Now.Hour && DateTime.Now.Hour < 6)
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, 22, 0, 0);
            }
            Console.WriteLine(result.ToString("dd.MM.yyyy HH:mm:ss"));
            return result;
        }
    }
}
