using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class SubscriptionJSON
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        //public string _id { get; set; }
        //public string sub_plan { get; set; }
        [JsonProperty("sub_plan_name")]
        public string SubPlanName { get; set; }
        //public bool is_gift { get; set; }
        [JsonProperty("user")]
        public UserJSON User { get; set; }
        //public object sender { get; set; }
    }

    public class RootSubscriptionJSON
    {
        //public int _total { get; set; }
        [JsonProperty("subscriptions")]
        public List<SubscriptionJSON> Subscriptions { get; set; }
    }
}
