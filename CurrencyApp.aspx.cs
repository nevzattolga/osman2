using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Web.Script.Serialization;
using System.Linq;
using System.Web.Services;
using System.Collections.Generic;
using System.Web;
using HttpCookie = System.Web.HttpCookie;

namespace Knowizz.WebApplication.Layouts.Knowizz.WebApplication
{
    public partial class CurrencyApp : LayoutsPageBase
    {
        [Serializable]
        public class RootobjectReturn
        {
            public string CurrencyShortName { get; set; }
            public string BanknoteSelling { get; set; }
        }


        [Serializable]
        public class Rootobject
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
        }

        [Serializable]
        public class RootCurr
        {
            public Data Data { get; set; }
        }

        public class Data
        {
            public Currency[] Currency { get; set; }
        }

        public class Currency
        {
            public string CurrencyCode { get; set; }
            public DateTime RateDate { get; set; }
            public string SaleRate { get; set; }
            public string PurchaseRate { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Write(GetCurrency());
        }

        [WebMethod]
        public static string GetCurrency()
        {

            SPListItem item = null;
            SPSite thissite = SPContext.Current.Site;
            SPWeb thisweb = SPContext.Current.Web;
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite currentSite = new SPSite(thissite.ID))
                {
                    using (SPWeb currenWeb = currentSite.OpenWeb(thisweb.ID))
                    {
                        SPList list = currenWeb.Lists["DovizKuruSettings"];

                        IEnumerable<SPListItem> items = (from SPListItem a in list.Items
                                                         select a);
                        item = items.Where(x => x["Title"].ToString() == "DovizKuru").FirstOrDefault();


                    }
                }
            });


            HttpCookie CurrCook = HttpContext.Current.Request.Cookies["VakifCurr"];

            if (CurrCook == null)
            {
                RestClient client = new RestClient(item["tokenurl"].ToString());
                client.Timeout = -1;
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", item["Content-Type"].ToString());
                request.AddParameter("client_id", item["client_id"].ToString());
                request.AddParameter("client_secret", item["client_secret"].ToString());
                request.AddParameter("grant_type", item["grant_type"].ToString());
                //request.AddParameter("scope", "oob");
                IRestResponse response = client.Execute(request);
                Rootobject des = JsonConvert.DeserializeObject<Rootobject>(response.Content);

                HttpContext.Current.Response.Cookies["VakifCurr"].Value = des.token_type + " " + des.access_token;
                HttpContext.Current.Response.Cookies["VakifCurr"].Expires = DateTime.Now.AddSeconds(des.expires_in).AddMinutes(-5);
                CurrCook = HttpContext.Current.Request.Cookies["VakifCurr"];
            }
            ///DateTime.Now.AddSeconds(des.expires_in).ToString()



            string date = DateTime.Now.ToString(("yyyy-MM-ddTHH:mm:ss") + ("zzz"));
            //Console.WriteLine(date);

            var newclient = new RestClient(item["geturl"].ToString());
            newclient.Timeout = -1;
            var newrequest = new RestRequest(Method.POST);
            newrequest.AddHeader("Content-Type", "application/json");
            newrequest.AddHeader("Authorization", CurrCook.Value);
            var body = "{ \"ValidityDate\": \"" + date + "\" } ";
            newrequest.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse newresponse = newclient.Execute(newrequest);
            RootCurr curr = JsonConvert.DeserializeObject<RootCurr>(newresponse.Content);


            string returnJson = JsonConvert.SerializeObject(new
            {
                currencyDate = DateTime.Now.ToString("dd.MM.yyyy"),
                data = new List<RootobjectReturn>()
                {
                    new RootobjectReturn {BanknoteSelling =  curr.Data.Currency.Where(x => x.CurrencyCode == "USD").FirstOrDefault().PurchaseRate, CurrencyShortName =   curr.Data.Currency.Where(x => x.CurrencyCode == "USD").FirstOrDefault().CurrencyCode },
                    new RootobjectReturn {BanknoteSelling =  curr.Data.Currency.Where(x => x.CurrencyCode == "EUR").FirstOrDefault().PurchaseRate, CurrencyShortName =   curr.Data.Currency.Where(x => x.CurrencyCode == "EUR").FirstOrDefault().CurrencyCode }
                }
            });

            //var json = new JavaScriptSerializer().Serialize(curr);
            return returnJson;

        }
    }
}
