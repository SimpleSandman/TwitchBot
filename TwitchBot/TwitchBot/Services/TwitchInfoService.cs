using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models.JSON;

namespace TwitchBot.Services
{
    public class TwitchInfoService
    {
        private TwitchBotConfigurationSection _botConfig;

        public TwitchInfoService(TwitchBotConfigurationSection botConfig)
        {
            _botConfig = botConfig;
        }

        /// <summary>
        /// Get a full list of chatters broken up by each type
        /// </summary>
        /// <returns></returns>
        public async Task<List<List<string>>> GetChatterListByType()
        {
            // Grab user's chatter info (viewers, mods, etc.)
            ChatterInfoJSON chatterInfo = await TaskJSON.GetChatters(_botConfig.Broadcaster, _botConfig.TwitchClientId);

            // Make list of available chatters by chatter type
            // ToDo: Categorize each list with username and chatter type
            List<List<string>> availChatterTypeList = new List<List<string>>();

            if (chatterInfo.chatter_count > 0)
            {
                Chatters chatters = chatterInfo.chatters; // get list of chatters

                if (chatters.viewers.Count() > 0)
                    availChatterTypeList.Add(chatters.viewers);
                if (chatters.moderators.Count() > 0)
                    availChatterTypeList.Add(chatters.moderators);
                if (chatters.global_mods.Count() > 0)
                    availChatterTypeList.Add(chatters.global_mods);
                if (chatters.admins.Count() > 0)
                    availChatterTypeList.Add(chatters.admins);
                if (chatters.staff.Count() > 0)
                    availChatterTypeList.Add(chatters.staff);
            }

            return availChatterTypeList;
        }

        /// <summary>
        /// Get a full list of chatters broken up by each type
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetChatterList()
        {
            // Grab user's chatter info (viewers, mods, etc.)
            ChatterInfoJSON chatterInfo = await TaskJSON.GetChatters(_botConfig.Broadcaster, _botConfig.TwitchClientId);

            // Make list of available chatters
            List<string> availChatterList = new List<string>();

            if (chatterInfo.chatter_count > 0)
            {
                Chatters chatters = chatterInfo.chatters; // get list of chatters

                if (chatters.viewers.Count() > 0)
                    availChatterList.AddRange(chatters.viewers);
                if (chatters.moderators.Count() > 0)
                    availChatterList.AddRange(chatters.moderators);
                if (chatters.global_mods.Count() > 0)
                    availChatterList.AddRange(chatters.global_mods);
                if (chatters.admins.Count() > 0)
                    availChatterList.AddRange(chatters.admins);
                if (chatters.staff.Count() > 0)
                    availChatterList.AddRange(chatters.staff);
            }

            return availChatterList;
        }

        /// <summary>
        /// Check if viewer is a follower via HttpResponseMessage
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> CheckFollowerStatus(string username)
        {
            return await TaskJSON.GetFollowerStatus(_botConfig.Broadcaster.ToLower(), _botConfig.TwitchClientId, username);
        }
    }
}
