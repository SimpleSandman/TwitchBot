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
            ChatterInfoJSON chatterInfo = await TwitchApi.GetChatters(_botConfig.TwitchClientId);

            // Make list of available chatters by chatter type
            // ToDo: Categorize each list with username and chatter type
            List<List<string>> availChatterTypeList = new List<List<string>>();

            if (chatterInfo.ChatterCount > 0)
            {
                Chatters chatters = chatterInfo.Chatters; // get list of chatters

                if (chatters.Viewers.Count() > 0)
                    availChatterTypeList.Add(chatters.Viewers);
                if (chatters.Moderators.Count() > 0)
                    availChatterTypeList.Add(chatters.Moderators);
                if (chatters.GlobalMods.Count() > 0)
                    availChatterTypeList.Add(chatters.GlobalMods);
                if (chatters.Admins.Count() > 0)
                    availChatterTypeList.Add(chatters.Admins);
                if (chatters.Staff.Count() > 0)
                    availChatterTypeList.Add(chatters.Staff);
            }

            return availChatterTypeList;
        }

        /// <summary>
        /// Get a full list of chatters
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetChatterList()
        {
            // Grab user's chatter info (viewers, mods, etc.)
            ChatterInfoJSON chatterInfo = await TwitchApi.GetChatters(_botConfig.TwitchClientId);

            // Make list of available chatters
            List<string> availChatterList = new List<string>();

            if (chatterInfo.ChatterCount > 0)
            {
                Chatters chatters = chatterInfo.Chatters; // get list of chatters

                if (chatters.Viewers.Count() > 0)
                    availChatterList.AddRange(chatters.Viewers);
                if (chatters.Moderators.Count() > 0)
                    availChatterList.AddRange(chatters.Moderators);
                if (chatters.GlobalMods.Count() > 0)
                    availChatterList.AddRange(chatters.GlobalMods);
                if (chatters.Admins.Count() > 0)
                    availChatterList.AddRange(chatters.Admins);
                if (chatters.Staff.Count() > 0)
                    availChatterList.AddRange(chatters.Staff);
            }

            return availChatterList;
        }

        /// <summary>
        /// Check if viewer is a follower
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> CheckFollowerStatus(string username)
        {
            return await TwitchApi.GetFollowerStatus(username, _botConfig.TwitchClientId);
        }
    }
}
