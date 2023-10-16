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
    }
}
