using RestSharp;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace MenuSelector
{
    public class SlackNotifier
    {
        private readonly RestClient _restClient = new RestClient(ConfigurationManager.AppSettings["WebhookUrl"]) { Timeout = 10000 };

        private string Username { get; set; }

        public SlackNotifier(string username)
        {
            Username = username;
        }

        public async Task<HttpStatusCode> Notice(string message)
        {
            var channels = ConfigurationManager.AppSettings["Channels"].Split(',');

            foreach (var channel in channels)
            {
                var statusCode = await Notice(channel, message);
                if (CheckStatusCode(statusCode) == false)
                    return statusCode;
            }

            return HttpStatusCode.OK;
        }

        private async Task<HttpStatusCode> Notice(string channel, string message)
        {
            var request = new RestRequest
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json,
            };
            request.AddJsonBody(new
            {
                channel = channel,
                username = Username,
                attachments = new[]
                {
                    new
                    {
                        text = "<!here> " + message,
                        mrkdwn_in = new [] { "text" }
                    }
                }
            });

            var response = await _restClient.ExecuteTaskAsync(request);

            return response.StatusCode;
        }

        private bool CheckStatusCode(HttpStatusCode statusCode)
        {
            return HttpStatusCode.OK <= statusCode && statusCode < HttpStatusCode.Ambiguous;
        }
    }
}
