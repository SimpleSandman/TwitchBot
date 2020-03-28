using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The abstract class for the "Command Subsystem"
    /// </summary>
    public abstract class BaseFeature
    {
        protected IrcClient _irc;
        protected TwitchBotConfigurationSection _botConfig;
        protected readonly Dictionary<string, string> _rolePermission;

        public BaseFeature(IrcClient irc, TwitchBotConfigurationSection botConfig)
        {
            _irc = irc;
            _botConfig = botConfig;
            _rolePermission = new Dictionary<string, string>();
        }

        public bool IsRequestExecuted(TwitchChatter chatter)
        {
            string requestedCommand = chatter.Message;

            int spaceIndex = chatter.Message.IndexOf(" ");
            if (spaceIndex > 1)
            {
                requestedCommand = chatter.Message.Substring(0, spaceIndex);
            }

            bool validCommand = _rolePermission.TryGetValue(requestedCommand, out string role);

            if (validCommand)
            {
                if ((chatter.Badges.Contains("broadcaster"))
                    || (chatter.Badges.Contains("moderator") && (role == "mod" || role == "vip"))
                    || (chatter.Badges.Contains("vip") && role == "vip")
                    || string.IsNullOrEmpty(role))
                {
                    ExecCommand(chatter, requestedCommand);
                    return true;
                }
            }

            return false;
        }

        public abstract void ExecCommand(TwitchChatter chatter, string requestedCommand);
    }
}
