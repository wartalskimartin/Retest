using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharedLib.DataAccess;
using SharedLib.Models;
using System.Data;

namespace PushAPI.Services
{
    public class BibloxService
    {
        private readonly cockpitContext _dbcontext;

        public BibloxService(cockpitContext dbContext) 
        {
            _dbcontext = dbContext;
        }

        public async Task<List<ProductionData>> GetLatestNOKFrom(string standLoc, DateTime? startShift)
        {
            return await _dbcontext.ProductionData.Where(pd => pd.Loc == standLoc && pd.ProductionDateTime >= startShift && pd.Nok == 1 && !_dbcontext.QualityCardBilboxes.Any(qcb => qcb.ProductionDataNokId == pd.Id)).ToListAsync();
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

        public async Task<StandsList?> GetBilboxInformation(string bilboxLoc)
        {
            return await _dbcontext.StandsLists.FirstOrDefaultAsync(b => b.Stand == bilboxLoc);
        }

        public async Task<int?> GetQualityCounter(string locationNumber)
        {
            var locationNumberParameter = new SqlParameter("@locationNumber", SqlDbType.VarChar) { Value = locationNumber };
            List<QualityCounterResult> result = await _dbcontext.QualityCounterResult.FromSqlRaw($"EXEC [dbo].[getQualityCounterSP] @locationNumber", locationNumberParameter).ToListAsync();

            return result.FirstOrDefault()?.QualityCounter1;
        }
    }
}
