using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace MenuSelector
{
    public class MenuNotifier
    {
        private readonly RestClient _restClient = new RestClient(ConfigurationManager.AppSettings["WebhookUrl"]);

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
            var channels = ConfigurationManager.AppSettings["Channels"].Split(',');

            foreach (var channel in channels)
                await Notice(channel, menu);
        }

        private async Task Notice(string channel, string menu)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json,
            };
            request.AddJsonBody(new
            {
                channel = channel,
                username = "Today's Menu",
                attachments = new[]
				{
					new
					{
						text = "<!here> " + menu,
						mrkdwn_in = new [] { "text" }
					}
				}
            });
            var response = await _restClient.ExecuteTaskAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Logger.Log(string.Format("notice error ({0})", response.StatusCode));
            }
        }
    }
}
