using Microsoft.AspNetCore.Mvc;

namespace TwitchBot.Api.DTO
{
    public class BroadcasterConfig
    {
        [FromQuery(Name = "broadcasterName")]
        public string BroadcasterName { get; set; }
        [FromQuery(Name = "botName")]
        public string BotName { get; set; }
    }
}
