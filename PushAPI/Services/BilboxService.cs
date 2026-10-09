using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharedLib.DataAccess;
using SharedLib.DTO;
using SharedLib.Models;
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

        private async Task<List<string>> ExecuteSingleColumnProcedureAsync(string procedureName, params SqlParameter[] parameters)
        {
            await using var connection = _dbcontext.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;

            foreach (var parameter in parameters)
                command.Parameters.Add(parameter);

            var result = new List<string>();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                {
                    var value = reader.GetValue(0)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        result.Add(value);
                }
            }

            return result.Distinct().ToList();
        }

        public async Task<List<string>> GetOperatorNumbersByOrderOperation(int orderNumber, int operationNumber)
        {
            return await ExecuteSingleColumnProcedureAsync(
                "dbo.GetOperatorNumbersByOrderOperation",
                new SqlParameter("@BWAUNR", SqlDbType.Int) { Value = orderNumber },
                new SqlParameter("@BWAGNR", SqlDbType.Int) { Value = operationNumber });
        }

        public async Task<List<int>> GetOperationPredecessors(int orderNumber, int operationNumber)
        {
            var rawValues = await ExecuteSingleColumnProcedureAsync(
                "dbo.GetOperationPredecessors",
                new SqlParameter("@Order", SqlDbType.Int) { Value = orderNumber },
                new SqlParameter("@Operation", SqlDbType.Int) { Value = operationNumber });

            return rawValues
                .Select(x => int.TryParse(x, out var value) ? value : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();
        }

        public async Task<List<RetestKomponentVadyDTO>> GetKomponents()
        {
            return await _dbcontext.RetestKomponents
                .AsNoTracking()
                .OrderBy(k => k.Nazev)
                .Select(k => new RetestKomponentVadyDTO
                {
                    KomponentId = k.Id,
                    KomponentName = k.Nazev,
                    Vady = k.RetestVada
                        .OrderBy(v => v.Nazev)
                        .Select(v => new RetestVadaDTO
                        {
                            VadaId = v.Id,
                            KomponentId = v.RetestKomponentId,
                            VadaName = v.Nazev
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<List<ProductionData>> GetLatestNOKFrom(string standLoc, DateTime? startShift)
        {
            return await _dbcontext.ProductionData.AsNoTracking()
                .Where(pd => pd.Loc == standLoc && pd.ProductionDateTime >= startShift && pd.Nok == 1 && !_dbcontext.QualityCardBilboxes.Any(qcb => qcb.ProductionDataNokId == pd.Id))
                .OrderBy(pd => pd.Id).ToListAsync();
        }

        public async Task AddQualityCardBilbox(List<QualityCardBilbox> retestedNok)
        {
            await _dbcontext.QualityCardBilboxes.AddRangeAsync(retestedNok);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<List<QualityDefectDescriptionBilbox>> GetDefectCodes()
        {
            return await _dbcontext.QualityDefectDescriptionBilboxes.AsNoTracking().ToListAsync();
        }


        public async Task<List<QualityDefectDescriptionBilbox>> GetDefectCodesEn()
        {
            return await _dbcontext.QualityDefectDescriptionBilboxEns
                .Select(q => new QualityDefectDescriptionBilbox { IdQualityDefectDescription = q.Id, DefectCode = q.DefectCode, DefectDescription = q.DefectDescription })
                .AsNoTracking().ToListAsync();
        }

        public async Task<StandsList?> GetBilboxInformation(string bilboxLoc)
        {
            return await _dbcontext.StandsLists.AsNoTracking().FirstOrDefaultAsync(b => b.Stand == bilboxLoc);
        }

        public async Task<int?> GetQualityCounter(string locationNumber)
        {
            var locationNumberParameter = new SqlParameter("@locationNumber", SqlDbType.VarChar) { Value = locationNumber };
            List<QualityCounterResult> result = await _dbcontext.QualityCounterResult.FromSqlRaw($"EXEC [dbo].[getQualityCounterSP] @locationNumber", locationNumberParameter).ToListAsync();

            return result.FirstOrDefault()?.QualityCounter1;
        }

        public async Task<List<string>> GetHalls()
        {
            return await _dbcontext.StandsLists.AsNoTracking().Where(s => s.Hall != null).Select(s => s.Hall.ToString() ?? "").Distinct().ToListAsync();
        }

        public async Task<List<string>> GetRows(int hall)
        {
            return await _dbcontext.StandsLists.AsNoTracking().Where(s => s.Hall == hall).Select(s => s.StandRow).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetRows()
        {
            return await _dbcontext.StandsLists.AsNoTracking().Select(s => s.StandRow).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStands(int hall, string row)
        {
            return await _dbcontext.StandsLists.AsNoTracking().Where(s => s.Hall == hall && s.StandRow == row).Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStands(int hall)
        {
            return await _dbcontext.StandsLists.AsNoTracking().Where(s => s.Hall == hall).Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStands()
        {
            return await _dbcontext.StandsLists.AsNoTracking().Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetStandsByRows(string row)
        {
            return await _dbcontext.StandsLists.AsNoTracking().Where(s => s.StandRow == row).Select(s => s.Stand).Distinct().ToListAsync();
        }

        public async Task<int?> GetActualOperatorNumber(string standLoc, DateTime? startShift)
        {
            return await _dbcontext.ProductionData.AsNoTracking().Where(s => s.Loc == standLoc && s.ProductionDateTime >= startShift).OrderByDescending(p => p.Id).Select(s => s.OperatorNumber).FirstOrDefaultAsync();
        }


        /*---------------------------------Manual NOK recording----------------------------------------------------*/

        public async Task<ProductionData> GetLastProductionData(string standLoc, DateTime startShift)
        {
            return await _dbcontext.ProductionData.AsNoTracking().Where(q => q.Loc == standLoc && q.ProductionDateTime >= startShift).OrderByDescending(q => q.Id).FirstOrDefaultAsync() ?? new();
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
