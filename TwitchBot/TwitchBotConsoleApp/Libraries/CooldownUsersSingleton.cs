using System;
using System.Collections.Generic;
using System.Linq;

using TwitchBotShared.Models;

namespace TwitchBotConsoleApp.Libraries
{
    public class CooldownUsersSingleton
    {
        private static volatile CooldownUsersSingleton _instance;
        private static object _syncRoot = new object();

        private List<CooldownUser> _cooldownUsers = new List<CooldownUser>();

        private CooldownUsersSingleton() { }

        public static CooldownUsersSingleton Instance
        {
            get
            {
                // first check
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        // second check
                        if (_instance == null)
                            _instance = new CooldownUsersSingleton();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Put a cooldown for a user on a command
        /// </summary>
        /// <param name="chatter"></param>
        /// <param name="cooldown"></param>
        public void AddCooldown(TwitchChatter chatter, DateTime cooldown, string command)
        {
            if (cooldown > DateTime.Now)
            {
                _cooldownUsers.Add(new CooldownUser
                {
                    Username = chatter.Username,
                    Cooldown = cooldown,
                    Command = command,
                    Warned = false
                });
            }

            if (command == "!wrongsong")
            {
                // Allow the user to request another song in case if cooldown exists
                CooldownUser songRequestCooldown =
                    _cooldownUsers.FirstOrDefault(u => u.Username == chatter.Username && u.Command == "!ytsr");

                if (songRequestCooldown != null)
                {
                    _cooldownUsers.Remove(songRequestCooldown);
                }
            }
        }

        /// <summary>
        /// Checks if a user/command is on a cooldown from a particular command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public bool IsCommandOnCooldown(string command, TwitchChatter chatter, IrcClient irc, bool hasGlobalCooldown = false)
        {
            CooldownUser cooldown = null;

            if (!hasGlobalCooldown)
                cooldown = _cooldownUsers.FirstOrDefault(u => u.Username == chatter.Username && u.Command == command);
            else
                cooldown = _cooldownUsers.FirstOrDefault(u => u.Command == command);

            if (cooldown == null) return false;
            else if (cooldown.Cooldown < DateTime.Now)
            {
                _cooldownUsers.Remove(cooldown);
                return false;
            }

            if (!cooldown.Warned)
            {
                string specialCooldownMessage = "";

                // ToDo: Find more graceful way to prevent spam of commands with a global cooldown
                if (!hasGlobalCooldown)
                {
                    cooldown.Warned = true; // prevent spamming cooldown message per personal cooldown
                    specialCooldownMessage = "a PERSONAL";
                }
                else
                {
                    specialCooldownMessage = "a GLOBAL";
                }

                string timespanMessage = "";
                TimeSpan timespan = cooldown.Cooldown - DateTime.Now;

                if (timespan.Minutes > 0)
                    timespanMessage = $"{timespan.Minutes} minute(s) and {timespan.Seconds} second(s)";
                else if (timespan.Seconds == 0)
                    timespanMessage = $"{timespan.Milliseconds} millisecond(s)";
                else
                    timespanMessage = $"{timespan.Seconds} second(s)";

                irc.SendPublicChatMessage($"The {command} command is currently on {specialCooldownMessage} cooldown @{chatter.DisplayName} for {timespanMessage}");
            }

            return true;
        }
    }
}
