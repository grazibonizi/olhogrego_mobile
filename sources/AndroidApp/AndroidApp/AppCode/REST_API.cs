using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AndroidApp.AppCode
{
    public class REST_API
    {
        private const string url = "http://olhogregocore.eu-gb.mybluemix.net/api/occurence";
        public const int TimeoutInMinutes = 2;
        private const int maxResponseContentBufferSize = 256000;
        private const string accept_json = "application/x-www-form-urlencoded";

        public static async Task<Tuple<bool,string>> RegisterOccurenceAsync(Occurence occurence)
        {

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(TimeoutInMinutes);
                    client.MaxResponseContentBufferSize = maxResponseContentBufferSize;
                    HttpResponseMessage response;
                    string content;

                    //var json = JsonConvert.SerializeObject(occurence);
                    //var postBody = new StringContent(json, Encoding.UTF8, accept_json);
                    var postBody = new StringContent(string.Format(
                        "user_cpf=\"{0}\"&dt_start=\"{1}\"&dt_end=\"{2}\"&lat={3}&lng={4}&level={5}&src=\"{6}\"",
                        occurence.user_cpf,
                        occurence.dt_start,
                        occurence.dt_end,
                        occurence.lat,
                        occurence.lng,
                        occurence.level,
                        occurence.src));

                    response = await client.PostAsync(url, postBody);
                    content = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return new Tuple<bool, string>(true, "");
                    else
                        return new Tuple<bool, string>(false, response.StatusCode.ToString());
                }
            }
            catch(Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }
    }
}