using System;
using System.Threading.Tasks;

using TwitchBot.Commands.Features;
using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;
using TwitchBot.Threads;

namespace TwitchBot.Commands
{
    /// <summary>
    /// The "Facade" class for the command system
    /// </summary>
    public class CommandSystem
    {
        private readonly BankFeature _bank;
        private readonly TwitterFeature _twitter;
        private readonly SongRequestFeature _songRequestFeature;
        private readonly LibVLCSharpPlayerFeature _libVLCSharpPlayerFeature;
        private readonly TwitchChannelFeature _twitchChannelFeature;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public CommandSystem(IrcClient irc, TwitchBotConfigurationSection botConfig, bool hasTwitterInfo, System.Configuration.Configuration appConfig, 
            BankService bank, SongRequestBlacklistService songRequestBlacklist, LibVLCSharpPlayer libVLCSharpPlayer, SongRequestSettingService songRequestSetting)
        {
            _bank = new BankFeature(irc, botConfig, bank);
            _twitter = new TwitterFeature(irc, botConfig, appConfig, hasTwitterInfo);
            _songRequestFeature = new SongRequestFeature(irc, botConfig, appConfig, songRequestBlacklist, libVLCSharpPlayer, songRequestSetting);
            _libVLCSharpPlayerFeature = new LibVLCSharpPlayerFeature(irc, botConfig, appConfig, libVLCSharpPlayer);
            _twitchChannelFeature = new TwitchChannelFeature(irc, botConfig);
        }

        public async Task ExecRequest(TwitchChatter chatter)
        {
            try
            {
                if (_bank.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (_twitter.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (_songRequestFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (_libVLCSharpPlayerFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (_twitchChannelFeature.IsRequestExecuted(chatter))
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
