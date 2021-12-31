using System;

namespace TaiwanStockTrading
{
    class Common
    {
        public static string GmtToTaipei(DateTime o)
        {
            // 調整為台北時區
            TimeZoneInfo easternStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
            DateTimeOffset timeInEST = TimeZoneInfo.ConvertTime(o, easternStandardTime);
            string dtDesc = timeInEST.ToString("yyyy-MM-dd HH:mm:ss");
            return dtDesc;
        }
    }
}
