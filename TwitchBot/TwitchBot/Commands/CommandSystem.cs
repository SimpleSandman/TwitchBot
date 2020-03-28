using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Commands.Features;
using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Commands
{
    /// <summary>
    /// The "Facade" class for the command system
    /// </summary>
    public class CommandSystem
    {
        private readonly BankFeature _bank;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public CommandSystem(IrcClient irc, TwitchBotConfigurationSection botConfig, BankService bank)
        {
            _bank = new BankFeature(irc, botConfig, bank);
        }

        public async Task ExecRequest(TwitchChatter chatter)
        {
            try
            {
                if (_bank.IsRequestExecuted(chatter))
                {
                    return;
                }
                // ToDo: Add a "else if" for each feature
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CommandSystem", "ExecRequest(TwitchChatter)", false, "N/A", chatter.Message);
            }
        }
    }
}
