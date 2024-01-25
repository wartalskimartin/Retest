using Microsoft.EntityFrameworkCore;
using SharedLib.DataAccess;
using SharedLib.Models;

namespace PushAPI.Services
{
    public class BibloxService
    {
        private readonly cockpitContext _dbcontext;

        public BibloxService(cockpitContext dbContext) 
        {
            _dbcontext = dbContext;
        }

        public async Task<List<ProductionData>> GetLatestShiftNOK(string standLoc, DateTime? startActualShift)
        {
            return await _dbcontext.ProductionData.Where(pd => pd.Loc == standLoc && pd.ProductionDateTime >= startActualShift && pd.Nok == 1).ToListAsync();
        }

        public async Task AddQualityCardBilbox(List<QualityCardBilbox> retestedNok)
        {
            await _dbcontext.QualityCardBilboxes.AddRangeAsync(retestedNok);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<List<QualityDefectDescriptionBilbox>> GetDefectCodes()
        {
            return await _dbcontext.QualityDefectDescriptionBilboxes.ToListAsync();
        }
    }
}
