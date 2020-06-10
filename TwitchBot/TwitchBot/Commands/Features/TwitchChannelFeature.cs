using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;
using TwitchBot.Threads;

using TwitchBotDb.DTO;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "____" feature
    /// </summary>
    public sealed class TwitchChannelFeature : BaseFeature
    {
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public TwitchChannelFeature(IrcClient irc, TwitchBotConfigurationSection botConfig) : base(irc, botConfig)
        {
            _rolePermission.Add("!", "");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!game":
                        ShowCurrentTwitchGame(chatter);
                        break;
                    case "!title":
                        ShowCurrentTwitchTitle(chatter);
                        break;
                    default:
                        if (requestedCommand == "!")
                        {
                            //await OtherCoolThings(chatter);
                            break;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TemplateFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }

        /// <summary>
        /// Display the current game/category for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async void ShowCurrentTwitchGame(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"We're currently playing \"{TwitchStreamStatus.CurrentCategory}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "ShowCurrentTwitchGame(TwitchChatter)", false, "!game");
            }
        }

        /// <summary>
        /// Display the current title for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async void ShowCurrentTwitchTitle(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"The title of this stream is \"{TwitchStreamStatus.CurrentTitle}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "ShowCurrentTwitchTitle(TwitchChatter)", false, "!title");
            }
        }

        /* ToDo: Insert new methods here */
    }
}
