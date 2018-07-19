using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MenuSelector
{
    public class NoticeTime
    {
        public int NoticeHour { get; set; }
        public int NoticeMin { get; set; }
        public bool NoticeSent { get; set; }
    }

    public class MenuNotifier
    {
        private SlackNotifier _notifier = new SlackNotifier("Today's Menu");

        private DayOfWeek LastDayOfWeek { get; set; }

        private List<NoticeTime> _noticeTimes = new List<NoticeTime>();

        public bool Initialize()
        {
            var noticeTimes = ConfigurationManager.AppSettings["NoticeTime"].Split(',');

            foreach (var time in noticeTimes)
            {
                var noticeTime = time.Split(':');
                int hour;
                int min;
                if (int.TryParse(noticeTime[0], out hour) == false)
                    return false;

                if (int.TryParse(noticeTime[1], out min) == false)
                    return false;

                _noticeTimes.Add(new NoticeTime()
                {
                    NoticeHour = hour,
                    NoticeMin = min,
                    NoticeSent = false,
                });
            }

            if (_noticeTimes.Any() == false)
                return false;

            return true;
        }

        public bool IsTimeUp()
        {
            var now = DateTime.Now;
            if (now.DayOfWeek != LastDayOfWeek)
            {
                _noticeTimes.ForEach(x => x.NoticeSent = false);
            }

            if (_noticeTimes.All(x => x.NoticeSent))
                return false;

            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    var noticeTime =
                        _noticeTimes.FirstOrDefault(
                           x => x.NoticeHour == now.Hour && x.NoticeMin == now.Minute &&
                                                  x.NoticeSent == false);
                    if (noticeTime != null)
                    {
                        noticeTime.NoticeSent = true;
                        break;
                    }

                    return false;


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
