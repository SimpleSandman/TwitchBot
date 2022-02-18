using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class SubscriptionJSON
    {
        [JsonProperty("broadcaster_id")]
        public string BroadcasterId { get; set; }

        [JsonProperty("broadcaster_login")]
        public string BroadcasterLogin { get; set; }

        [JsonProperty("broadcaster_name")]
        public string BroadcasterName { get; set; }

        [JsonProperty("gifter_id")]
        public string GifterId { get; set; }

        [JsonProperty("gifter_login")]
        public string GifterLogin { get; set; }

        [JsonProperty("gifter_name")]
        public string GifterName { get; set; }

        [JsonProperty("is_gift")]
        public bool IsGift { get; set; }

        [JsonProperty("plan_name")]
        public string PlanName { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }
    }

    public class RootSubscriptionJSON
    {
        [JsonProperty("data")]
        public List<SubscriptionJSON> Subscriptions { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
