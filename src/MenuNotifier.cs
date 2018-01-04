using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace MenuSelector
{
    public class MenuNotifier
    {
        private SlackNotifier _notifier = new SlackNotifier("Today's Menu");

        private int _noticeHour;
        private int _noticeMin;
        private DayOfWeek LastDayOfWeek { get; set; }

        public bool Initialize()
        {
            var noticeTime = ConfigurationManager.AppSettings["NoticeTime"].Split(':');

            if (int.TryParse(noticeTime[0], out _noticeHour) == false)
                return false;

            if (int.TryParse(noticeTime[1], out _noticeMin) == false)
                return false;

            return true;
        }

        public bool IsTimeUp()
        {
            var now = DateTime.Now;
            if (now.DayOfWeek == LastDayOfWeek)
                return false;

            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    if (now.Hour != _noticeHour || now.Minute != _noticeMin)
                        return false;
                    break;

                default:
                    return false;
            }

            LastDayOfWeek = now.DayOfWeek;

            return true;
        }

        public async Task Notice(string menu)
        {
            var statusCode = await _notifier.Notice(menu);
            if (statusCode != HttpStatusCode.OK)
            {
                Logger.Log(string.Format("notice error ({0})", statusCode));
            }
        }
    }
}
