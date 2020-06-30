using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The abstract class for the "Command Subsystem"
    /// </summary>
    public abstract class BaseFeature
    {
        private readonly CooldownUsersSingleton _cooldownUsersInstance = CooldownUsersSingleton.Instance;
        private readonly BotModeratorSingleton _botModeratorInstance = BotModeratorSingleton.Instance;

        protected IrcClient _irc;
        protected TwitchBotConfigurationSection _botConfig;
        protected readonly Dictionary<string, List<ChatterType>> _rolePermission;

        public BaseFeature(IrcClient irc, TwitchBotConfigurationSection botConfig)
        {
            _irc = irc;
            _botConfig = botConfig;
            _rolePermission = new Dictionary<string, List<ChatterType>>();
        }

        public async Task<bool> IsRequestExecuted(TwitchChatter chatter) 
        {
            string requestedCommand = CommandToolbox.ParseChatterCommand(chatter);
            bool validCommand = _rolePermission.ContainsKey(requestedCommand);

            if (validCommand && !_cooldownUsersInstance.IsCommandOnCooldown(requestedCommand, chatter, _irc))
            {
                (bool, DateTime) commandResult = await ExecCommand(chatter, requestedCommand);
                _cooldownUsersInstance.AddCooldown(chatter, commandResult.Item2);
                return commandResult.Item1;
            }

            return false;
        }

        /// <summary>
        /// Returns the chatter type needed to determine specific permissions (i.e. for ambiguous command names)
        /// </summary>
        /// <param name="chatter">The user in the chat</param>
        /// <returns></returns>
        protected ChatterType DetermineChatterPermissions(TwitchChatter chatter)
        {
            if (chatter.Badges.Contains("broadcaster"))
            {
                return ChatterType.Broadcaster;
            }
            else if ((chatter.Badges.Contains("moderator") || _botModeratorInstance.IsBotModerator(chatter.TwitchId)))
            {
                return ChatterType.Moderator;
            }
            else if (chatter.Badges.Contains("vip"))
            {
                return ChatterType.VIP;
            }

            return ChatterType.Viewer;
        }

        /// <summary>
        /// Execute a command from a feature
        /// </summary>
        /// <param name="chatter">The user in the chat</param>
        /// <param name="requestedCommand">The command that is being requested</param>
        public abstract Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand);
    }
}
