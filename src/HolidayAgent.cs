using System;
using System.Collections.Generic;
using System.Configuration;
using RestSharp;

namespace MenuSelector
{
    public class HolidayAgent
    {
        private readonly RestClient _restClient = new RestClient("http://apis.data.go.kr/B090041/openapi/service/SpcdeInfoService") { Timeout = 10000 };

        private readonly List<int> _holidays = new List<int>();

        private int LastMonth { get; set; }

        public bool CheckWorkDay(DateTime now)
        {
            if (LastMonth != now.Month)
            {
                GetHolidays(now);
                LastMonth = now.Month;
            }

            switch (now.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    return false;
            }

            return _holidays.Contains(now.Day) == false;
        }

        class DayInfo
        {

        }

        private void GetHolidays(DateTime now)
        {
            var request = new RestRequest("getRestDeInfo");
            request.AddQueryParameter("solYear", now.Year.ToString());
            request.AddQueryParameter("solMonth", now.Month.ToString());
            request.AddQueryParameter("_type", "json");
            request.AddQueryParameter("numOfRows", "30");
            request.AddQueryParameter("ServiceKey", ConfigurationManager.AppSettings["ServiceKey"]);

            var response = _restClient.Get<DayInfo>(request);
        }
    }
}
