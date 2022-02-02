using System;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Commands.Features;
using TwitchBotShared.Config;
using TwitchBotShared.Models;
using TwitchBotShared.Threads;

namespace TwitchBotShared.Commands
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
        private readonly DiscordFeature _discordFeature;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public CommandSystem(IrcClient irc, TwitchBotConfigurationSection botConfig, Configuration appConfig, BankService bank, 
            SongRequestBlacklistService songRequestBlacklist, LibVLCSharpPlayer libVLCSharpPlayer, SongRequestSettingService songRequestSetting,
            SpotifyWebClient spotify, TwitchInfoService twitchInfo, FollowerService follower, GameDirectoryService gameDirectory, InGameUsernameService ign,
            ManualSongRequestService manualSongRequest, QuoteService quote, PartyUpService partyUp, DiscordNetClient discord)
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
            _discordFeature = new DiscordFeature(irc, botConfig, discord);
        }

        public async Task ExecRequestAsync(TwitchChatter chatter)
        {
            try
            {
                if (await _bank.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _followerFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _generalFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _inGameNameFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _joinStreamerFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _libVLCSharpPlayerFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _miniGameFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _multiLinkUserFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _partyUpFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _quoteFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _reminderFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _songRequestFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _spotifyFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _twitchChannelFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _twitter.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
                else if (await _discordFeature.IsRequestExecutedAsync(chatter))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "CommandSystem", "ExecRequest(TwitchChatter)", false, "N/A", chatter.Message);
            }
        }
    }
}
