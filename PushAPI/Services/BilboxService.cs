using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharedLib.DataAccess;
using SharedLib.DTO;
using SharedLib.Models;
using System;
using System.Data;

namespace PushAPI.Services
{
    public class BilboxService
    {
        private readonly cockpitContext _dbcontext;

        public BilboxService(cockpitContext dbContext) 
        {
            _dbcontext = dbContext;
        }

        public async Task<List<ProductionData>> GetLatestNOKFrom(string standLoc, DateTime? startShift)
        {
            return await _dbcontext.ProductionData
                .Where(pd => pd.Loc == standLoc && pd.ProductionDateTime >= startShift && pd.Nok == 1 && !_dbcontext.QualityCardBilboxes.Any(qcb => qcb.ProductionDataNokId == pd.Id))
                .OrderBy(pd => pd.DatetimeServer).ToListAsync();
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

        public async Task<List<string>> GetHalls()
        {
            return await _dbcontext.StandsLists.Where(s => s.Hall != null).Select(s => s.Hall.ToString() ?? "").Distinct().ToListAsync();
        }

        public async Task<List<string>> GetRows(int hall)
        {
            return await _dbcontext.StandsLists.Where(s => s.Hall == hall).Select(s => s.StandRow).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetRows()
        {
            return await _dbcontext.StandsLists.Select(s => s.StandRow).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStands(int hall, string row)
        {
            return await _dbcontext.StandsLists.Where(s => s.Hall == hall && s.StandRow == row).Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStands(int hall)
        {
            return await _dbcontext.StandsLists.Where(s => s.Hall == hall).Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStands()
        {
            return await _dbcontext.StandsLists.Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStandsByRows(string row)
        {
            return await _dbcontext.StandsLists.Where(s => s.StandRow == row).Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<int?> GetActualOperatorNumber(string standLoc)
        {
            return await _dbcontext.ProductionData.Where(s => s.Loc == standLoc).OrderByDescending(p => p.DatetimeServer).Select(s => s.OperatorNumber).FirstOrDefaultAsync();
        }


        /*---------------------------------Manual NOK recording----------------------------------------------------*/

        public async Task<ProductionData> GetLastProductionData(string standLoc)
        {
            return await _dbcontext.ProductionData.Where(q => q.Loc == standLoc).OrderByDescending(q => q.DatetimeServer).FirstOrDefaultAsync() ?? new();
        }

        public async Task<List<QualityCardBilbox>> GetQualityCardBilboxData(string standLoc, DateTime? startShift)
        {
            return await _dbcontext.QualityCardBilboxes.Where(q => q.ProductionDataNokId == null && q.WorkingPlaceNumber == standLoc && q.DatetimeServer >= startShift).ToListAsync();
        }

        public async Task InsertQualityCardBilbox(QualityCardBilbox qcb)
        {
            await _dbcontext.QualityCardBilboxes.AddAsync(qcb);
            await _dbcontext.SaveChangesAsync();
        }
        public async Task InsertQualityCardBilbox(List<QualityCardBilbox> qualityCards)
        {
            await _dbcontext.QualityCardBilboxes.AddRangeAsync(qualityCards);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdateQualityCardBilbox(QualityCardBilbox current, QualityCardBilbox updated)
        {
            _dbcontext.Entry(current).CurrentValues.SetValues(updated);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task RemoveQualityCardBilbox(QualityCardBilbox qcb)
        {
            _dbcontext.QualityCardBilboxes.Remove(qcb);
            await _dbcontext.SaveChangesAsync();
        }

        /*---------------------------------Show history by stands----------------------------------------------------*/

        public async Task<int> GetStandsHistory(DateTime startShift, string stand)
        {
            return await _dbcontext.QualityCardBilboxes
                .Where(q => q.ProductionDataNokId != null && q.DateTimeRecord >= startShift && q.WorkingPlaceNumber == stand)
                .CountAsync();
        }
    }
}
