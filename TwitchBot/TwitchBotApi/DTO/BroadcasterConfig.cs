using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchBotApi.DTO
{
    public class BroadcasterConfig
    {
        [FromQuery(Name = "broadcasterName")]
        public string BroadcasterName { get; set; }
        [FromQuery(Name = "botName")]
        public string BotName { get; set; }
    }
}
