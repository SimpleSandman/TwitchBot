using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

using TwitchBotDb.DTO;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "____" feature
    /// </summary>
    public sealed class TemplateFeature : BaseFeature
    {
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public TemplateFeature(IrcClient irc, TwitchBotConfigurationSection botConfig) : base(irc, botConfig)
        {
            _rolePermission.Add("!", "");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            switch (requestedCommand)
            {
                case "!":
                    //await Deposit(chatter);
                    break;
                default:
                    if (requestedCommand == "!")
                    {
                        //await CheckFunds(chatter);
                        break;
                    }

                    break;
            }
        }

        /* ToDo: Insert new methods here */
    }
}
