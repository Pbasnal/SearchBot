using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;

namespace SearchBot
{
    public class OnlineSearch
    {
        //API Key
        private static string API_KEY = "AIzaSyBXQQpGniXFCgHAee_G6suWqY2ejMImWDo";

        //The custom search engine identifier
        private static string cx = "000695698406768629142:wgrhkhb3fua";

        public static CustomsearchService Service = new CustomsearchService(
            new BaseClientService.Initializer
            {
                ApplicationName = "ISBNBookCsutomSearch",
                ApiKey = API_KEY,
            });

        public async Task<IList<Result>> SearchGoogle(string query)
        {
            try
            {
                var listRequest = Service.Cse.List(query);
                listRequest.Cx = cx;

                var search = listRequest.Execute();

                return search.Items;
            }
            catch (Exception ex)
            {}

            return new List<Result>();
        }
    }
}