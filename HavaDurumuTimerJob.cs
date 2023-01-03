using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Knowizz.HavaDurumuTimerJ
{
    class HavaDurumuTimerJob : SPJobDefinition
    {
        public HavaDurumuTimerJob() : base()
        {

        }

        public HavaDurumuTimerJob(string jobName, SPService service) : base(jobName, service, null, SPJobLockType.None)
        {
            this.Title = "Hava Durumu Timer Job";
        }

        public HavaDurumuTimerJob(string jobName, SPWebApplication webapp) : base(jobName, webapp, null, SPJobLockType.ContentDatabase)
        {
            this.Title = "Hava Durumu Timer Job";
        }

        public override void Execute(Guid targetInstanceId)
        {
            StringBuilder batchBuilder = new StringBuilder();
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite currentSite = new SPSite(this.WebApplication.Sites[0].Url))
                {
                    using (SPWeb currentWeb = currentSite.OpenWeb())
                    {
                        SPList list = currentWeb.Lists["WeatherList"];
                        
                        IEnumerable<SPListItem> items = (from SPListItem a in list.Items
                                                         select a);
                        string apiKey = items.Where(x => x["Title"].ToString() == "ApiKey").FirstOrDefault()["CityID"].ToString();
                        foreach (SPListItem item in items.Where(x=>x["Title"].ToString() != "ApiKey"))
                        {
                            using (WebClient wc = new WebClient { Encoding = Encoding.UTF8 })
                            {
                                string cityID = item["CityID"].ToString();
                                string weather = wc.DownloadString("https://api.openweathermap.org/data/2.5/weather?id=" + cityID + "&units=metric&lng=tr&appid=" + apiKey + "");
                                var obj = JsonConvert.DeserializeObject<Root>(weather);

                                item["Temp"] = obj.main.temp;
                                item["Icon"] = obj.weather[0].icon;

                                item.Update();

                            }

                            Thread.Sleep(1500);
                        }

                    }
                }
            });
        }
    }

    public class Root
    {
        public List<Weather> weather { get; set; }
        public Main main { get; set; }
        public int id { get; set; }
    }

    public class Weather
    {
        public string icon { get; set; }
    }
    public class Main
    {
        public string temp { get; set; }
    }
}
