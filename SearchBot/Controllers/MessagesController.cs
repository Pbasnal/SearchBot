using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SearchBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (activity.Type == ActivityTypes.Message)
            {
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                var botReply = "";
                IList<Google.Apis.Customsearch.v1.Data.Result> searchResult = new List<Google.Apis.Customsearch.v1.Data.Result>();

                var StLUIS = await GetEntityFromLUIS(activity.Text);
                if (StLUIS.topScoringIntent != null)
                {
                    switch (StLUIS.topScoringIntent.intent)
                    {
                        case "greeting":
                            botReply = "Hello !! ask me to search something";
                            break;
                        case "search":
                            searchResult = await SearchOnline(StLUIS.query);
                            foreach (var search in searchResult)
                            {
                                botReply += search.Link + "\n";
                            }
                            break;
                        default:
                            botReply = "Sorry, I am not getting you...";
                            break;
                    }
                }

                // return our reply to the user
                Activity reply = activity.CreateReply(botReply);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<IList<Google.Apis.Customsearch.v1.Data.Result>> SearchOnline(string query)
        {
            var onlineSearch = new OnlineSearch();
            IList<Google.Apis.Customsearch.v1.Data.Result> searchResults = await onlineSearch.SearchGoogle(query);
            if (searchResults == null)
            {
                return new List<Google.Apis.Customsearch.v1.Data.Result>();
            }
            else
            {
                return searchResults;
            }
        }

        private static async Task<LuisResult> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            var Data = new LuisResult();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/c8a873cf-5015-4a13-a0d5-bff430c6d0d3?subscription-key=dbbfd65241254925b4dbe3d30302c084&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<LuisResult>(JsonDataResponse);
                }
            }
            return Data;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}