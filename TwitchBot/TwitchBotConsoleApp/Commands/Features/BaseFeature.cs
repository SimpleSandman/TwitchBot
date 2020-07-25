using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Libraries;

using TwitchBotUtil.Config;
using TwitchBotUtil.Enums;
using TwitchBotUtil.Models;

namespace TwitchBotConsoleApp.Commands.Features
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
        protected readonly Dictionary<string, CommandPermission> _rolePermission;

        public BaseFeature(IrcClient irc, TwitchBotConfigurationSection botConfig)
        {
            _irc = irc;
            _botConfig = botConfig;
            _rolePermission = new Dictionary<string, CommandPermission>();
        }

        public async Task<bool> IsRequestExecuted(TwitchChatter chatter) 
        {
            string requestedCommand = ParseChatterCommandName(chatter);
            bool validCommand = _rolePermission.TryGetValue(requestedCommand, out CommandPermission permission);

            if (validCommand 
                && DetermineChatterPermissions(chatter) >= permission.General
                && !_cooldownUsersInstance.IsCommandOnCooldown(requestedCommand, chatter, _irc))
            {
                (bool, DateTime) commandResult = await ExecCommand(chatter, requestedCommand);
                _cooldownUsersInstance.AddCooldown(chatter, commandResult.Item2, ParseChatterCommandName(chatter));
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
        /// Used for messages that require a boolean operation
        /// </summary>
        /// <param name="message">Valid operations: {on, off, true, false}</param>
        /// <returns></returns>
        protected bool SetBooleanFromMessage(string message)
        {
            if (message == "on" || message == "true" || message == "yes")
            {
                return true;
            }
            else if (message == "off" || message == "false" || message == "no")
            {
                return false;
            }
            else
            {
                throw new Exception("Couldn't find specified message");
            }
        }

        /// <summary>
        /// Save modified settings in the app config. Make sure to adjust the corresponding variable in the TwitchBotConfigurationSection
        /// </summary>
        /// <param name="savedValue">The new value that is replacing the property's current value</param>
        /// <param name="propertyName">The name of the property that is being modified</param>
        /// <param name="appConfig"></param>
        protected void SaveAppConfigSettings(string savedValue, string propertyName, Configuration appConfig)
        {
            appConfig.AppSettings.Settings.Remove(propertyName);
            appConfig.AppSettings.Settings.Add(propertyName, savedValue);
            appConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("TwitchBotConfiguration");
        }

        /// <summary>
        /// Parse out the command from the chatter's message
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        protected string ParseChatterCommandName(TwitchChatter chatter)
        {
            int spaceIndex = chatter.Message.IndexOf(" ") > 0
                ? chatter.Message.IndexOf(" ")
                : chatter.Message.Length;

            return chatter.Message.Substring(0, spaceIndex).ToLower();
        }

        /// <summary>
        /// Get the parameter value(s) in the chatter's message that is denoted after the first space in the IRC message
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        protected string ParseChatterCommandParameter(TwitchChatter chatter)
        {
            return chatter?.Message?.Substring(chatter.Message.IndexOf(" ") + 1) ?? "";
        }

        /// <summary>
        /// Get the requested username from the chatter's message
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        protected string ParseChatterMessageUsername(TwitchChatter chatter)
        {
            if (chatter.Message.IndexOf("@") > 0)
            {
                return chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
            }

            return chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);
        }

        /// <summary>
        /// Check if the chatter has the minimum elevated permissons needed
        /// </summary>
        /// <param name="requestedCommand"></param>
        /// <param name="chatterPermission"></param>
        /// <param name="rolePermissions"></param>
        /// <param name="isElevated"></param>
        /// <returns></returns>
        protected bool HasPermission(string requestedCommand, ChatterType chatterPermission, Dictionary<string, CommandPermission> rolePermissions, bool isElevated = false)
        {
            rolePermissions.TryGetValue(requestedCommand, out CommandPermission permissions);

            if (isElevated)
                return chatterPermission >= permissions.Elevated;
            else
                return chatterPermission >= permissions.General;
        }

        protected bool ReactionCommand(IrcClient irc, string origUser, string recipient, string msgToSelf, string action, string addlMsg = "")
        {
            // check if user is trying to use a command on themselves
            if (origUser.ToLower() == recipient.ToLower())
            {
                irc.SendPublicChatMessage($"{msgToSelf} @{origUser}");
                return true;
            }

            irc.SendPublicChatMessage($"{origUser} {action} @{recipient} {addlMsg}");
            return false;
        }

        protected string Effectiveness()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int effectiveLvl = rnd.Next(3); // between 0 and 2

            switch (effectiveLvl)
            {
                case 0:
                    return "It's super effective!";
                case 1:
                    return "It wasn't very effective";
                default:
                    return "It had no effect";
            }
        }

        /// <summary>
        /// Execute a command from a feature
        /// </summary>
        /// <param name="chatter">The user in the chat</param>
        /// <param name="requestedCommand">The command that is being requested</param>
        public abstract Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand);
    }
}
