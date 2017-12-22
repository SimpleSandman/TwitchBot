using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TwitchBot.Models.JSON
{
    public class SubscribedUserJSON
    {
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        //public string _id { get; set; }
        //public string sub_plan { get; set; }
        [JsonProperty("sub_plan_name")]
        public string SubPlanName { get; set; }
        //public bool is_gift { get; set; }
        [JsonProperty("user")]
        public UserJSON User { get; set; }
        //public object sender { get; set; }
    }
}
