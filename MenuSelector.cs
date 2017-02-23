using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RestSharp;

namespace MenuSelector
{
    public sealed class MenuSelector
    {
        [Serializable]
        public class Menu
        {
            [XmlAttribute]
            public string Name { get; set; }
        }

        [Serializable]
        [XmlRoot]
        public class MenuList
        {
            [XmlElement]
            public Menu[] Menu { get; set; }
        }

        private readonly RestClient _restClient = new RestClient(ConfigurationManager.AppSettings["WebhookUrl"]);
        private readonly List<string> _menu = new List<string>();
        private readonly Queue<string> _shuffledMenu = new Queue<string>();

        private int _noticeHour;
        private int _noticeMin;
        private DayOfWeek LastDayOfWeek { get; set; }

        public bool Initialize()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var noticeTime = ConfigurationManager.AppSettings["NoticeTime"].Split(':');

            if (int.TryParse(noticeTime[0], out _noticeHour) == false)
                return false;

            if (int.TryParse(noticeTime[1], out _noticeMin) == false)
                return false;

            return LoadMenu();
        }

        private bool LoadMenu()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(MenuList));
                var reader = new StreamReader("Menu.xml");
                var list = (MenuList)serializer.Deserialize(reader);
                reader.Close();

                foreach (var menu in list.Menu)
                    _menu.Add(menu.Name);

                if (_menu.Any() == false)
                {
                    Log("menu is empty");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log(e.Message);

                return false;
            }
        }

        private void Log(string message)
        {
            using (var file = new StreamWriter("MenuSelector.Log", true))
            {
                file.WriteLine(message);
            }
        }

        public async Task Update()
        {
            ShuffleIfNeeded();

            if (IsTimeUp())
            {
                await Notice();
            }
        }

        private void ShuffleIfNeeded()
        {
            if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday && _shuffledMenu.Count != 0)
                return;

            var menus = _menu.ToArray();

            FisherYates.Shuffle(menus);

            _shuffledMenu.Clear();
            foreach (var menu in menus)
            {
                _shuffledMenu.Enqueue(menu);
            }
        }

        private bool IsTimeUp()
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

        private async Task Notice()
        {
            var channels = ConfigurationManager.AppSettings["Channels"].Split(',');
            var menu = Choose();

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
                Log(string.Format("notice error ({0})", response.StatusCode));
            }
        }

        private string Choose()
        {
            return _shuffledMenu.Dequeue();
        }

        private class FisherYates
        {
            private static Random Random { get; set; }

            static FisherYates()
            {
                Random = new Random(Guid.NewGuid().GetHashCode() ^ Guid.NewGuid().GetHashCode());
            }

            public static void Shuffle<T>(T[] array)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    var j = i + Random.Next(array.Length - i);
                    var temp = array[j];
                    array[j] = array[i];
                    array[i] = temp;
                }

                //Queue<T> queue = new Queue<T>(array);
                //return queue;
            }
        }
    }
}
