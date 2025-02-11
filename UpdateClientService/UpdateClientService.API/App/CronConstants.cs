using System;

namespace UpdateClientService.API.App
{
    public class CronConstants
    {
        public const string AtMinute0Every12thHour = "0 */12 * * *";

        public static readonly string AtRandomMinuteEvery12thHour =
            string.Format("{0} */12 * * *", new Random().Next(0, 59));

        public static string EveryXHours(int hours)
        {
            return string.Format("0 */{0} * * *", hours);
        }

        public static string AtRandomMinuteEveryXHours(int hours)
        {
            return string.Format("{0} */{1} * * *", new Random().Next(0, 59), hours);
        }
    }
}