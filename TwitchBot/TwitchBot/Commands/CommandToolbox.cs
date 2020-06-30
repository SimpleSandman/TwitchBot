using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;

namespace TwitchBot.Commands
{
    public static class CommandToolbox
    {
        /// <summary>
        /// Used for messages that require a boolean operation
        /// </summary>
        /// <param name="message">Valid operations: {on, off, true, false}</param>
        /// <returns></returns>
        public static bool SetBooleanFromMessage(string message)
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
        public static void SaveAppConfigSettings(string savedValue, string propertyName, System.Configuration.Configuration appConfig)
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
        public static string ParseChatterCommand(TwitchChatter chatter)
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
        public static string ParseChatterCommandParameter(TwitchChatter chatter)
        {
            return chatter?.Message?.Substring(chatter.Message.IndexOf(" ") + 1) ?? "";
        }

        /// <summary>
        /// Check if the chatter has the minimum permissons needed
        /// </summary>
        /// <param name="requestedCommand"></param>
        /// <param name="chatterPermission"></param>
        /// <param name="rolePermissions"></param>
        /// <returns></returns>
        public static bool HasAccessToCommand(string requestedCommand, ChatterType chatterPermission, Dictionary<string, List<ChatterType>> rolePermissions)
        {
            rolePermissions.TryGetValue(requestedCommand, out List<ChatterType> permissions);
            return chatterPermission >= permissions.Min();
        }

        public static bool ReactionCmd(IrcClient irc, string origUser, string recipient, string msgToSelf, string action, string addlMsg = "")
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

        public static string Effectiveness()
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
        /// Get the requested username from the chatter's message
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public static string ParseChatterMessageName(TwitchChatter chatter)
        {
            if (chatter.Message.IndexOf("@") > 0)
            {
                return chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
            }
            
            return chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);
        }
    }
}
