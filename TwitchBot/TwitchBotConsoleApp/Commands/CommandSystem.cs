using System;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Commands.Features;
using TwitchBotConsoleApp.Libraries;
using TwitchBotConsoleApp.Threads;

using TwitchBotDb.Services;

using TwitchBotShared.Config;
using TwitchBotShared.Models;

namespace TwitchBotConsoleApp.Commands
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
        private readonly FollowerFeature _followerFeature;
        private readonly GeneralFeature _generalFeature;
        private readonly InGameNameFeature _inGameNameFeature;
        private readonly RefreshFeature _reminderFeature;
        private readonly SpotifyFeature _spotifyFeature;
        private readonly QuoteFeature _quoteFeature;
        private readonly JoinStreamerFeature _joinStreamerFeature;
        private readonly MinigameFeature _miniGameFeature;
        private readonly MultiLinkUserFeature _multiLinkUserFeature;
        private readonly PartyUpFeature _partyUpFeature;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public CommandSystem(IrcClient irc, TwitchBotConfigurationSection botConfig, Configuration appConfig, BankService bank, 
            SongRequestBlacklistService songRequestBlacklist, LibVLCSharpPlayer libVLCSharpPlayer, SongRequestSettingService songRequestSetting,
            SpotifyWebClient spotify, TwitchInfoService twitchInfo, FollowerService follower, GameDirectoryService gameDirectory, InGameUsernameService ign,
            ManualSongRequestService manualSongRequest, QuoteService quote, PartyUpService partyUp)
        {
            _bank = new BankFeature(irc, botConfig, bank);
            _followerFeature = new FollowerFeature(irc, botConfig, twitchInfo, follower, appConfig);
            _generalFeature = new GeneralFeature(irc, botConfig, twitchInfo, appConfig);
            _inGameNameFeature = new InGameNameFeature(irc, botConfig, twitchInfo, gameDirectory, ign);
            _joinStreamerFeature = new JoinStreamerFeature(irc, botConfig, twitchInfo, gameDirectory);
            _libVLCSharpPlayerFeature = new LibVLCSharpPlayerFeature(irc, botConfig, appConfig, libVLCSharpPlayer);
            _miniGameFeature = new MinigameFeature(irc, botConfig, bank, follower, twitchInfo);
            _multiLinkUserFeature = new MultiLinkUserFeature(irc, botConfig);
            _partyUpFeature = new PartyUpFeature(irc, botConfig, twitchInfo, gameDirectory, partyUp);
            _quoteFeature = new QuoteFeature(irc, botConfig, quote);
            _reminderFeature = new RefreshFeature(irc, botConfig, twitchInfo, gameDirectory);
            _songRequestFeature = new SongRequestFeature(irc, botConfig, appConfig, songRequestBlacklist, libVLCSharpPlayer, songRequestSetting, manualSongRequest, bank, spotify);
            _spotifyFeature = new SpotifyFeature(irc, botConfig, spotify);
            _twitchChannelFeature = new TwitchChannelFeature(irc, botConfig, gameDirectory);
            _twitter = new TwitterFeature(irc, botConfig, appConfig);
        }

        public async Task ExecRequest(TwitchChatter chatter)
        {
            try
            {
                if (await _bank.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _followerFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _generalFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _inGameNameFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _joinStreamerFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _libVLCSharpPlayerFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _miniGameFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _multiLinkUserFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _partyUpFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _quoteFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _reminderFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _songRequestFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _spotifyFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _twitchChannelFeature.IsRequestExecuted(chatter))
                {
                    return;
                }
                else if (await _twitter.IsRequestExecuted(chatter))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CommandSystem", "ExecRequest(TwitchChatter)", false, "N/A", chatter.Message);
            }
        }
    }
}
