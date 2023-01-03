using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knowizz.WebApplication.Layouts.Knowizz.WebApplication
{
    public partial class GetWeather : LayoutsPageBase
    {
        private static SPSite thissite = SPContext.Current.Site;
        private static SPWeb thisweb = SPContext.Current.Web;


        protected void Page_Load(object sender, EventArgs e)
        {
            string what = HttpContext.Current.Request.QueryString["what"];
            if (what == "cities")
            {
                HttpContext.Current.Response.Write(GetCities());
            }
            else if (what == "weather")
            {
                string city = HttpContext.Current.Request.QueryString["city"];
                HttpContext.Current.Response.Write(GetWeathers(city));
            }
        }

        public static string GetWeathers(string city)
        {
            string json = string.Empty;
            SPListItem item = null;
            using (SPSite currentSite = new SPSite(thissite.ID))
            {
                using (SPWeb currentWeb = currentSite.OpenWeb(thisweb.ID))
                {
                    SPList listWeather = currentWeb.Lists["WeatherList"];

                    item = (from SPListItem a in listWeather.Items
                            where a["Title"].ToString().Equals(city)
                            select a).FirstOrDefault();
                }
            }
            List<ResultWeather> result = new List<ResultWeather>();
            if (item != null)
            {
                string icon = GetIcon(item["Icon"].ToString());

                result.Add(new ResultWeather { temp = item["Temp"].ToString(), icon = icon });
            }
            json = JsonConvert.SerializeObject(result);


            return json;
        }

        protected static string GetIcon(string icon)
        {
            switch (icon)
            {
                case "01d": icon = "31"; break;
                case "02d": icon = "27"; break;
                case "03d": icon = "26"; break;
                case "04d": icon = "26"; break;
                case "09d": icon = "6"; break;
                case "10d": icon = "8"; break;
                case "11d": icon = "1"; break;
                case "13d": icon = "15"; break;
                case "50d": icon = "20"; break;

                case "01n": icon = "31"; break;
                case "02n": icon = "27"; break;
                case "03n": icon = "26"; break;
                case "04n": icon = "26"; break;
                case "09n": icon = "6"; break;
                case "10n": icon = "8"; break;
                case "11n": icon = "1"; break;
                case "13n": icon = "15"; break;
                case "50n": icon = "20"; break;

                default: icon = "0"; break;
            }
            return icon;
        }

        public static string GetCities()
        {

            string json = string.Empty;
            IEnumerable<SPListItem> items = null;
            using (SPSite currentSite = new SPSite(thissite.ID))
            {
                using (SPWeb currentWeb = currentSite.OpenWeb(thisweb.ID))
                {
                    SPList listWeather = currentWeb.Lists["WeatherList"];

                    items = (from SPListItem a in listWeather.Items
                             where a["Title"].ToString() != "ApiKey"
                             select a);
                }
            }
            List<ResultCity> result = new List<ResultCity>();
            if (items != null)
            {
                foreach (var item in items)
                {
                    result.Add(new ResultCity { city = item.Title });
                }
            }
            json = JsonConvert.SerializeObject(result);


            return json;
        }

    }

    public class ResultCity
    {
        public string city { get; set; }
    }

    public class ResultWeather
    {
        public string temp { get; set; }
        public string icon { get; set; }
    }
}



