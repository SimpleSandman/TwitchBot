using System;
using System.Configuration;

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
        /// Get the parameter value(s) in the chatter's message that is denoted after the first space in the IRC message
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public static string ParseChatterCommandParameter(TwitchChatter chatter)
        {
            return chatter?.Message?.Substring(chatter.Message.IndexOf(" ") + 1) ?? "";
        }
    }
}
