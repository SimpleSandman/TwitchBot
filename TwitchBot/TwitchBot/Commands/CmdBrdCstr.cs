using System;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;
using TwitchBot.Threads;

using TwitchBotDb.Models;

namespace TwitchBot.Commands
{
    public class CmdBrdCstr
    {
        private IrcClient _irc;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private TwitchInfoService _twitchInfo;
        private GameDirectoryService _gameDirectory;
        private InGameUsernameService _ign;
        private TwitchStreamStatus _twitchStreamStatus;
        private TwitterClient _twitter = TwitterClient.Instance;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private BossFightSingleton _bossFightSettingsInstance = BossFightSingleton.Instance;
        private CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;

        public CmdBrdCstr(IrcClient irc, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig, 
            TwitchInfoService twitchInfo, GameDirectoryService gameDirectory, InGameUsernameService ign, 
            TwitchStreamStatus twitchStreamStatus)
        {
            _irc = irc;
            _botConfig = botConfig;
            _appConfig = appConfig;
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _ign = ign;
            _twitchStreamStatus = twitchStreamStatus;
        }

        /// <summary>
        /// Display bot settings
        /// </summary>
        public async void CmdBotSettings()
        {
            try
            {
                _irc.SendPublicChatMessage($"Auto tweets set to \"{_botConfig.EnableTweets}\" "
                    + $">< Auto display songs set to \"{_botConfig.EnableDisplaySong}\" "
                    + $">< Currency set to \"{_botConfig.CurrencyType}\" "
                    + $">< Stream Latency set to \"{_botConfig.StreamLatency} second(s)\" "
                    + $">< Regular follower hours set to \"{_botConfig.RegularFollowerHours}\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdBotSettings()", false, "!settings");
            }
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        public async void CmdExitBot()
        {
            try
            {
                _irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdExitBot()", false, "!exit");
            }
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        public async Task<bool> CmdEnableManualSrMode(bool isManualSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("Song requests enabled");
                isManualSongRequestAvail = true;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableManualSrMode()", false, "!rsrmode on");
            }

            return isManualSongRequestAvail;
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        public async Task<bool> CmdDisableManualSrMode(bool isManualSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("Song requests disabled");
                isManualSongRequestAvail = false;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableSRMode()", false, "!rsrmode off");
            }

            return isManualSongRequestAvail;
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        public async Task<bool> CmdEnableYouTubeSrMode(bool isYouTubeSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("YouTube song requests enabled");
                isYouTubeSongRequestAvail = true;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableYouTubeSrMode()", false, "!ytsrmode on");
            }

            return isYouTubeSongRequestAvail;
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        public async Task<bool> CmdDisableYouTubeSrMode(bool isYouTubeSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("YouTube song requests disabled");
                isYouTubeSongRequestAvail = false;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableYouTubeSrMode()", false, "!ytsrmode off");
            }

            return isYouTubeSongRequestAvail;
        }

        /// <summary>
        /// Manually send a tweet
        /// </summary>
        /// <param name="hasTwitterInfo">Check if user has provided the specific twitter credentials</param>
        /// <param name="message">Chat message from the user</param>
        public async void CmdTweet(bool hasTwitterInfo, string message)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                    _irc.SendPublicChatMessage(_twitter.SendTweet(message.Replace("!tweet ", "")));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdTweet(bool, string)", false, "!tweet");
            }
        }

        /// <summary>
        /// Enables displaying songs from Spotify into the IRC chat
        /// </summary>
        public async void CmdEnableDisplaySongs()
        {
            try
            {
                _botConfig.EnableDisplaySong = true;
                _appConfig.AppSettings.Settings.Remove("enableDisplaySong");
                _appConfig.AppSettings.Settings.Add("enableDisplaySong", "true");
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                Console.WriteLine("Auto display songs is set to [" + _botConfig.EnableDisplaySong + "]");
                _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic display Spotify songs is set to \"" + _botConfig.EnableDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableDisplaySongs()", false, "!displaysongs on");
            }
        }

        /// <summary>
        /// Disables displaying songs from Spotify into the IRC chat
        /// </summary>
        public async void CmdDisableDisplaySongs()
        {
            try
            {
                _botConfig.EnableDisplaySong = false;
                _appConfig.AppSettings.Settings.Remove("enableDisplaySong");
                _appConfig.AppSettings.Settings.Add("enableDisplaySong", "false");
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                Console.WriteLine("Auto display songs is set to [" + _botConfig.EnableDisplaySong + "]");
                _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic display Spotify songs is set to \"" + _botConfig.EnableDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableDisplaySongs()", false, "!displaysongs off");
            }
        }

        public async Task CmdLive(bool hasTwitterInfo)
        {
            try
            {
                if (!_twitchStreamStatus.IsLive)
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
                else if (!_botConfig.EnableTweets)
                    _irc.SendPublicChatMessage("Tweets are disabled at the moment");
                else if (string.IsNullOrEmpty(_twitchStreamStatus.CurrentCategory) || string.IsNullOrEmpty(_twitchStreamStatus.CurrentTitle))
                    _irc.SendPublicChatMessage("Unable to pull the Twitch title/category at the moment. Please try again in a few seconds");
                else if (_botConfig.EnableTweets && hasTwitterInfo)
                {
                    string tweetResult = _twitter.SendTweet($"Live on Twitch playing {_twitchStreamStatus.CurrentCategory} "
                        + $"\"{_twitchStreamStatus.CurrentTitle}\" twitch.tv/{_botConfig.Broadcaster}");

                    _irc.SendPublicChatMessage($"{tweetResult} @{_botConfig.Broadcaster}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdLive(bool)", false, "!live");
            }
        }

        public async Task CmdRefreshReminders()
        {
            try
            {
                await Threads.ChatReminder.RefreshReminders();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRefreshReminders()", false, "!refreshreminders");
            }
        }

        public async Task CmdRefreshBossFight()
        {
            try
            {
                // Check if any fighters are queued or fighting
                if (_bossFightSettingsInstance.Fighters.Count > 0)
                {
                    _irc.SendPublicChatMessage($"A boss fight is either queued or in progress @{_botConfig.Broadcaster}");
                    return;
                }

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);

                // During refresh, make sure no fighters can join
                _bossFightSettingsInstance.RefreshBossFight = true;
                await _bossFightSettingsInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);
                _bossFightSettingsInstance.RefreshBossFight = false;

                _irc.SendPublicChatMessage($"Boss fight settings refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRefreshBossFight()", false, "!refreshbossfight");
            }
        }

        public async void CmdSetRegularFollowerHours(string message)
        {
            try
            {
                bool validInput = int.TryParse(message.Substring(17), out int regularHours);
                if (!validInput)
                {
                    _irc.SendPublicChatMessage($"I can't process the time you've entered. " + 
                        $"Please insert positive hours @{_botConfig.Broadcaster}");
                    return;
                }
                else if (regularHours < 1)
                {
                    _irc.SendPublicChatMessage($"Please insert positive hours @{_botConfig.Broadcaster}");
                    return;
                }

                _botConfig.RegularFollowerHours = regularHours;
                _appConfig.AppSettings.Settings.Remove("regularFollowerHours");
                _appConfig.AppSettings.Settings.Add("regularFollowerHours", regularHours.ToString());
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                Console.WriteLine($"Regular followers are set to {_botConfig.RegularFollowerHours}");
                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster} : Regular followers now need {_botConfig.RegularFollowerHours} hours");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdSetRegularFollowerHours(string)", false, "!setregularhours");
            }
        }

        public async Task CmdSetGameIgn(string message)
        {
            try
            {
                string gameIgn = message.Substring(message.IndexOf(" ") + 1);

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);
                InGameUsername ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId, game);

                if (ign == null || (ign != null && ign.GameId == null))
                {
                    await _ign.CreateInGameUsername(game.Id, _broadcasterInstance.DatabaseId, gameIgn);

                    _irc.SendPublicChatMessage($"Yay! You've set your IGN for {gameTitle} to \"{gameIgn}\"");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsername(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your IGN for {gameTitle} to \"{gameIgn}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdSetGameIgn(string)", false, "!setgameign");
            }
        }

        public async Task CmdSetGenericIgn(string message)
        {
            try
            {
                string gameIgn = message.Substring(message.IndexOf(" ") + 1);

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                InGameUsername ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId);

                if (ign == null)
                {
                    await _ign.CreateInGameUsername(null, _broadcasterInstance.DatabaseId, gameIgn);

                    _irc.SendPublicChatMessage($"Yay! You've set your generic IGN to \"{gameIgn}\"");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsername(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your generic IGN to \"{gameIgn}\"");
                }                
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdSetGenericIgn(string)", false, "!setgenericign");
            }
        }

        public async Task CmdDeleteIgn()
        {
            try
            {
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);
                InGameUsername ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId, game);

                if (ign != null && ign.GameId != null)
                {
                    await _ign.DeleteInGameUsername(ign.Id, _broadcasterInstance.DatabaseId);

                    _irc.SendPublicChatMessage($"Successfully deleted IGN set for the category \"{game.Title}\"");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Wasn't able to find an IGN to delete for the category \"{game.Title}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDeleteIgn()", false, "!deleteign");
            }
        }

        public async Task CmdRefreshCommands()
        {
            try
            {
                await _customCommandInstance.LoadCustomCommands(_botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);

                _irc.SendPublicChatMessage($"Your commands have been refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRefreshCommands()", false, "!refreshcommands");
            }
        }
    }
}
