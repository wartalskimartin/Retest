using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushAPI.Services
{
    public static class ProductionDataShiftRecognizer
    {
        public static DateTime GetActualShiftStartHour()
        {
            DateTime result;

            if (6 <= DateTime.Now.Hour && DateTime.Now.Hour < 14)
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
            }
            else if (14 <= DateTime.Now.Hour && DateTime.Now.Hour < 22)
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 0, 0);
            }
            else if (22 <= DateTime.Now.Hour && DateTime.Now.Hour <= 23)
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 22, 0, 0);
            }
            else //if (0 <= DateTime.Now.Hour && DateTime.Now.Hour < 6)
            {
                result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 22, 0, 0).AddDays(-1);
            }

            return result;
        }

        public static DateTime GetPreviousShiftStartHour()
        {
            DateTime result = GetActualShiftStartHour();

            return result.AddHours(-8);
        }
    }
}
