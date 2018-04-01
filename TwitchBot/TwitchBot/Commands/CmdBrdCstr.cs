using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Extensions;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

namespace TwitchBot.Commands
{
    public class CmdBrdCstr
    {
        private IrcClient _irc;
        private Moderator _modInstance = Moderator.Instance;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _broadcasterId;
        private SongRequestBlacklistService _songRequest;
        private TwitchInfoService _twitchInfo;
        private GiveawayService _giveaway;
        private GameDirectoryService _gameDirectory;
        private TwitterClient _twitter = TwitterClient.Instance;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private Broadcaster _broadcasterInstance = Broadcaster.Instance;
        private BossFightSettings _bossFightSettingsInstance = BossFightSettings.Instance;

        public CmdBrdCstr(IrcClient irc, TwitchBotConfigurationSection botConfig, string connStr, int broadcasterId, 
            System.Configuration.Configuration appConfig, SongRequestBlacklistService songRequest, TwitchInfoService twitchInfo,
            GiveawayService giveaway, GameDirectoryService gameDirectory)
        {
            _irc = irc;
            _botConfig = botConfig;
            _connStr = connStr;
            _broadcasterId = broadcasterId;
            _appConfig = appConfig;
            _songRequest = songRequest;
            _twitchInfo = twitchInfo;
            _giveaway = giveaway;
            _gameDirectory = gameDirectory;
        }

        /// <summary>
        /// Display bot settings
        /// </summary>
        public void CmdBotSettings()
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
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdBotSettings()", false, "!botsettings");
            }
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        public void CmdExitBot()
        {
            try
            {
                _irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdExitBot()", false, "!exitbot");
            }
        }

        /// <summary>
        /// Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public void CmdEnableTweet(bool hasTwitterInfo)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = true;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "true");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableTweet(bool)", false, "!sendtweet on");
            }
        }

        /// <summary>
        /// Disables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public void CmdDisableTweet(bool hasTwitterInfo)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = false;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "false");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableTweet(bool)", false, "!sendtweet off");
            }
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdEnableManualSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = true;
                _irc.SendPublicChatMessage("Song requests enabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableManualSrMode(ref bool)", false, "!rbsrmode on");
            }
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdDisableManualSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = false;
                _irc.SendPublicChatMessage("Song requests disabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableSRMode(ref bool)", false, "!rbsrmode off");
            }
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdEnableYouTubeSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = true;
                _irc.SendPublicChatMessage("YouTube song requests enabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableYouTubeSrMode(ref bool)", false, "!ytsrmode on");
            }
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdDisableYouTubeSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = false;
                _irc.SendPublicChatMessage("YouTube song requests disabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableYouTubeSrMode(ref bool)", false, "!ytsrmode off");
            }
        }

        /// <summary>
        /// Manually send a tweet
        /// </summary>
        /// <param name="bolHasTwitterInfo">Check if user has provided the specific twitter credentials</param>
        /// <param name="message">Chat message from the user</param>
        public void CmdTweet(bool bolHasTwitterInfo, string message)
        {
            try
            {
                if (!bolHasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                    _irc.SendPublicChatMessage(_twitter.SendTweet(message.Replace("!tweet ", "")));
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdTweet(bool, string)", false, "!tweet");
            }
        }

        /// <summary>
        /// Enables displaying songs from Spotify into the IRC chat
        /// </summary>
        public void CmdEnableDisplaySongs()
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
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableDisplaySongs()", false, "!displaysongs on");
            }
        }

        /// <summary>
        /// Disables displaying songs from Spotify into the IRC chat
        /// </summary>
        public void CmdDisableDisplaySongs()
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
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableDisplaySongs()", false, "!displaysongs off");
            }
        }

        /// <summary>
        /// Grant viewer to moderator status for this bot's mod commands
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public void CmdAddBotMod(string message)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1); // grab user from message
                _modInstance.AddNewModToList(recipient.ToLower(), _broadcasterId, _connStr); // add user to mod list and add to db
                _irc.SendPublicChatMessage("@" + recipient + " is now able to use moderator features within " + _botConfig.BotName);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdAddBotMod(string)", false, "!addmod");
            }
        }

        /// <summary>
        /// Revoke moderator status from user for this bot's mods commands
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public void CmdDelBotMod(string message)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1); // grab user from message
                _modInstance.DeleteOldModFromList(recipient.ToLower(), _broadcasterId, _connStr); // delete user from mod list and remove from db
                _irc.SendPublicChatMessage("@" + recipient + " is not able to use moderator features within " + _botConfig.BotName + " any longer");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDelBotMod(string)", false, "!delmod");
            }
        }

        /// <summary>
        /// List bot moderators
        /// </summary>
        public void CmdListMod()
        {
            try
            {
                string listModMsg = "";

                if (_modInstance.ListMods.Count > 0)
                {
                    foreach (string name in _modInstance.ListMods)
                        listModMsg += name + " >< ";

                    listModMsg = listModMsg.Remove(listModMsg.Length - 3); // removed extra " >< "
                    _irc.SendPublicChatMessage("List of bot moderators (separate from channel mods): " + listModMsg);
                }
                else
                    _irc.SendPublicChatMessage("No one is ruling over me other than you @" + _botConfig.Broadcaster);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdListMod()", false, "!listmod");
            }
        }

        /// <summary>
        /// Add a giveaway for a user to post in the chat
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public void CmdAddGiveaway(string message)
        {
            int giveawayType = -1;
            string minRandNum = "-1";
            string maxRandNum = "0";
            bool isValidated = true; // used for nested "if" validation

            try
            {
                // get due date of giveaway
                int dueDateMsgIndex = message.GetNthCharIndex(' ', 4);
                if (dueDateMsgIndex > 0)
                {
                    string giveawayDateMsg = message.Substring(13, dueDateMsgIndex - 13); // MM-DD-YY hh:mm:ss [AM/PM]
                    if (DateTime.TryParse(giveawayDateMsg, out DateTime giveawayDate))
                    {
                        // get eligibility parameters for user types (using boolean bits)
                        int elgMsgIndex = -1; // get the index of the space separating the message and the parameter
                        for (int i = 5; i < 8; i++)
                        {
                            elgMsgIndex = message.GetNthCharIndex(' ', i);
                            if (elgMsgIndex == -1) break;
                        }

                        if (elgMsgIndex > 0)
                        {
                            string giveawayElgMsg = message.Substring(message.GetNthCharIndex(' ', 4) + 1, 7); // [mods] [regulars] [subscribers] [users]
                            if (giveawayElgMsg.Replace(" ", "").IsInt()
                                && giveawayElgMsg.Replace(" ", "").Length == 4
                                && !Regex.IsMatch(giveawayElgMsg, @"[2-9]"))
                            {
                                int[] elgList =
                                {
                                    int.Parse(giveawayElgMsg.Substring(0, 1)),
                                    int.Parse(giveawayElgMsg.Substring(2, 1)),
                                    int.Parse(giveawayElgMsg.Substring(4, 1)),
                                    int.Parse(giveawayElgMsg.Substring(6, 1))
                                };

                                // get giveaway type (1 = Keyword, 2 = Random Number)
                                if (int.TryParse(message.Substring(message.GetNthCharIndex(' ', 8) + 1, 1), out giveawayType))
                                {
                                    // get parameter of new giveaway (1 = [keyword], 2 = [min]-[max])
                                    int paramMsgIndex = message.GetNthCharIndex(' ', 10); // get the index of the space separating the message and the parameter
                                    if (paramMsgIndex > 0)
                                    {
                                        string giveawayParam = message.Substring(44, paramMsgIndex - 44);

                                        if (giveawayType == 2)
                                        {
                                            // check if min-max range can be validated
                                            if (Regex.IsMatch(giveawayParam, @"^\d{1,4}-\d{1,4}"))
                                            {
                                                int dashIndex = giveawayParam.IndexOf('-');

                                                minRandNum = giveawayParam.Substring(0, dashIndex); // min
                                                maxRandNum = giveawayParam.Substring(dashIndex + 1); // max

                                                if (int.Parse(minRandNum) > int.Parse(maxRandNum))
                                                {
                                                    isValidated = false;
                                                }
                                            }
                                            else
                                            {
                                                isValidated = false;
                                            }
                                        }

                                        if (isValidated)
                                        {
                                            // get message of new giveaway
                                            string giveawayText = message.Substring(paramMsgIndex + 1);

                                            // log new giveaway into db
                                            _giveaway.AddGiveaway(giveawayDate, giveawayText, _broadcasterId, elgList,
                                                giveawayType, giveawayParam, minRandNum, maxRandNum);

                                            _irc.SendPublicChatMessage($"Giveaway \"{giveawayText}\" has started!");
                                        }
                                        else
                                        {
                                            if (int.Parse(minRandNum) > int.Parse(maxRandNum))
                                                _irc.SendPublicChatMessage($"Giveaway random number parameter values were flipped @{_botConfig.Broadcaster}");
                                            else
                                                _irc.SendPublicChatMessage($"Giveaway random number parameter [min-max] was not given correctly @{_botConfig.Broadcaster}");
                                        }
                                    }
                                    else
                                    {
                                        if (giveawayType == 1)
                                            _irc.SendPublicChatMessage($"Giveaway keyword parameter was not given @{_botConfig.Broadcaster}");
                                        else if (giveawayType == 2)
                                            _irc.SendPublicChatMessage($"Giveaway random number parameter was not given @{_botConfig.Broadcaster}");
                                    }
                                }
                                else
                                {
                                    if (giveawayType == -1)
                                        _irc.SendPublicChatMessage($"Giveaway type was not given correctly @{_botConfig.Broadcaster}");
                                    else if (giveawayType != 1 || giveawayType != 2)
                                        _irc.SendPublicChatMessage($"Please use giveaway type (1 = Keyword or 2 = Random Number) @{_botConfig.Broadcaster}");
                                }
                            }
                            else
                            {
                                _irc.SendPublicChatMessage($"Eligibility parameters were not given correctly @{_botConfig.Broadcaster}");
                            }
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"Couldn't find space between eligibility parameters @{_botConfig.Broadcaster}");
                        }
                    }
                    else
                    {
                        _irc.SendPublicChatMessage($"Date and time parameters were not given correctly @{_botConfig.Broadcaster}");
                    }
                }
                else
                {
                    _irc.SendPublicChatMessage($"Couldn't find eligibility parameters @{_botConfig.Broadcaster}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdAddGiveaway(string)", false, "!addgiveaway");
            }
        }

        /// <summary>
        /// Edit giveaway details (date/time, message, giveaway type, or eligibility)
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public void CmdEditGiveaway(string message)
        {
            try
            {
                int reqGiveawayId = -1;
                string reqGiveawayIdMsg = message.Substring(17, message.GetNthCharIndex(' ', 2) - message.GetNthCharIndex(' ', 1) - 1);
                bool isValidGiveawayId = int.TryParse(reqGiveawayIdMsg, out reqGiveawayId);

                // Validate requested giveaway ID
                if (!isValidGiveawayId || reqGiveawayId < 0)
                    _irc.SendPublicChatMessage("Please use a positive whole number to find your giveaway ID");
                else
                {
                    // Check if giveaway ID exists
                    int giveawayId = _giveaway.GetGiveawayId(reqGiveawayId, _broadcasterId);

                    // Check if giveaway ID was retrieved
                    if (giveawayId == -1)
                        _irc.SendPublicChatMessage($"Cannot find the giveaway ID: {reqGiveawayId}");
                    else
                    {
                        bool isEditValid = false;
                        int inputType = -1;
                        DateTime giveawayDate = new DateTime();
                        int[] elgList = { };
                        int giveawayType = -1;
                        string giveawayTypeParam1 = "";
                        string giveawayTypeParam2 = "";

                        string giveawayInput = message.Substring(message.GetNthCharIndex(' ', 2) + 1);

                        /* Check if user wants to edit the date and time, message, giveaway type, or eligibility */
                        if (message.StartsWith("!editgiveawayDTE"))
                        {
                            inputType = 1;

                            // Get new due date of giveaway
                            if (!DateTime.TryParse(giveawayInput, out giveawayDate))
                                _irc.SendPublicChatMessage($"Please enter a valid date and time: [MM-DD-YYYY HH:MM:SS AM/PM] @{_botConfig.Broadcaster}");
                            else
                                isEditValid = true;
                        }
                        else if (message.StartsWith("!editgiveawayMSG"))
                        {
                            inputType = 2;

                            // Get new message for giveaway
                            if (string.IsNullOrWhiteSpace(giveawayInput))
                                _irc.SendPublicChatMessage($"Please enter a valid message @{_botConfig.Broadcaster}");
                            else
                                isEditValid = true;
                        }
                        else if (message.StartsWith("!editgiveawayELG"))
                        {
                            inputType = 3;

                            // Get new eligibility list for giveaway
                            string giveawayElg = message.Substring(message.GetNthCharIndex(' ', 2) + 1, 7); // [mods] [regulars] [subscribers] [users]
                            if (giveawayElg.Replace(" ", "").IsInt()
                                && giveawayElg.Replace(" ", "").Length == 4
                                && !Regex.IsMatch(giveawayElg, @"[2-9]"))
                            {
                                elgList = new int[] 
                                {
                                    int.Parse(giveawayElg.Substring(0, 1)),
                                    int.Parse(giveawayElg.Substring(2, 1)),
                                    int.Parse(giveawayElg.Substring(4, 1)),
                                    int.Parse(giveawayElg.Substring(6, 1))
                                };

                                isEditValid = true;
                            }
                            else
                                _irc.SendPublicChatMessage($"Please enter a valid message @{_botConfig.Broadcaster}");
                        }
                        else if (message.StartsWith("!editgiveawayTYP"))
                        {
                            inputType = 4;

                            // Get new giveaway type and param(s)
                            if (string.IsNullOrWhiteSpace(giveawayInput))
                                _irc.SendPublicChatMessage($"Please enter a valid message @{_botConfig.Broadcaster}");
                            else if (!int.TryParse(message.Substring(message.GetNthCharIndex(' ', 2) + 1, 1), out giveawayType) || (giveawayType != 1 && giveawayType != 2))
                                _irc.SendPublicChatMessage($"Please enter a valid giveaway type (1 = Keyword or 2 = Random Number) @{_botConfig.Broadcaster}");
                            else
                            {
                                int paramIndex1 = message.GetNthCharIndex(' ', 3);
                                int paramIndex2 = message.GetNthCharIndex(' ', 4);

                                if (giveawayType == 1 && paramIndex1 < 0)
                                    _irc.SendPublicChatMessage($"Please enter a valid giveaway parameter for a keyword @{_botConfig.Broadcaster}");
                                else if (giveawayType == 2 && paramIndex2 < 0)
                                    _irc.SendPublicChatMessage($"Please enter a valid giveaway parameter for a random number range @{_botConfig.Broadcaster}");
                                else
                                {
                                    if (giveawayType == 1)
                                    {
                                        giveawayTypeParam1 = message.Substring(paramIndex1 + 1);
                                        isEditValid = true;
                                    }
                                    else if (giveawayType == 2)
                                    {
                                        giveawayTypeParam1 = message.Substring(paramIndex1 + 1, paramIndex2 - paramIndex1 - 1);
                                        giveawayTypeParam2 = message.Substring(paramIndex2 + 1);

                                        bool isValidIntParam1 = !int.TryParse(giveawayTypeParam1, out int testParam1);
                                        bool isValidIntParam2 = !int.TryParse(giveawayTypeParam2, out int testParam2);

                                        if (isValidIntParam1 && isValidIntParam2)
                                            _irc.SendPublicChatMessage($"Cannot parse numbers correctly. Please enter whole numbers @{_botConfig.Broadcaster}");
                                        else if (testParam1 > testParam2)
                                            _irc.SendPublicChatMessage("Parameter 1 is greater than parameter 2. " 
                                                + $"Please either flip these values or enter new ones @{_botConfig.Broadcaster}");
                                        else
                                            isEditValid = true;
                                    }
                                }
                            }
                        }

                        // Update info based on input type
                        if (inputType == -1)
                            _irc.SendPublicChatMessage($"Please specify an option to edit a giveaway @{_botConfig.Broadcaster}");
                        else if (isEditValid)
                        {
                            _giveaway.UpdateGiveaway(inputType, giveawayDate, giveawayInput, elgList, giveawayType,
                                giveawayId, _broadcasterId, giveawayTypeParam1, giveawayTypeParam2);

                            _irc.SendPublicChatMessage($"Changes to giveaway ID: {reqGiveawayId} have been made @{_botConfig.Broadcaster}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEditCountdown(string)", false, "!editgiveaway");
            }
        }

        public void CmdAddSongRequestBlacklist(string message)
        {
            try
            {
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request @{_botConfig.Broadcaster}");
                    return;
                }
                
                string requestType = message.Substring(message.IndexOf(" ") + 1, 1);
                string request = message.Substring(requestIndex + 1);

                // Check if request is based on an artist or just a song by an artist
                if (requestType.Equals("1")) // blackout any song by this artist
                {
                    // check if song-specific request is being used for artist blackout
                    if (request.Count(c => c == '"') == 2
                        || request.Count(c => c == '<') == 1
                        || request.Count(c => c == '>') == 1)
                    {
                        _irc.SendPublicChatMessage($"Please use request type 2 for song-specific blacklist-restrictions @{_botConfig.Broadcaster}");
                        return;
                    }

                    List<SongRequestBlacklistItem> blacklist = _songRequest.GetSongRequestBlackList(_broadcasterId);
                    if (blacklist.Count > 0 && blacklist.Exists(b => b.Artist.Equals(request, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        _irc.SendPublicChatMessage($"This song is already on the blacklist @{_botConfig.Broadcaster}");
                        return;
                    }

                    int recordsAffected = _songRequest.AddArtistToBlacklist(request, _broadcasterId);

                    if (recordsAffected > 0)
                        _irc.SendPublicChatMessage($"The artist \"{request}\" has been added to the blacklist @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"I'm sorry. I'm not able to add this artist to the blacklist at this time @{_botConfig.Broadcaster}");
                }
                else if (requestType.Equals("2")) // blackout a song by an artist
                {
                    if (request.Count(c => c == '"') < 2 
                        || request.Count(c => c == '<') != 1 
                        || request.Count(c => c == '>') != 1)
                    {
                        _irc.SendPublicChatMessage($"Please surround the song title with \" (quotation marks) " + 
                            $"and the artist with \"<\" and \">\" @{_botConfig.Broadcaster}");
                        return;
                    }

                    int songTitleStartIndex = message.IndexOf('"');
                    int songTitleEndIndex = message.LastIndexOf('"');
                    int artistStartIndex = message.IndexOf('<');
                    int artistEndIndex = message.IndexOf('>');

                    string songTitle = message.Substring(songTitleStartIndex + 1, songTitleEndIndex - songTitleStartIndex - 1);
                    string artist = message.Substring(artistStartIndex + 1, artistEndIndex - artistStartIndex - 1);

                    // check if the request's exact song or artist-wide blackout-restriction has already been added
                    List<SongRequestBlacklistItem> blacklist = _songRequest.GetSongRequestBlackList(_broadcasterId);

                    if (blacklist.Count > 0)
                    { 
                        if (blacklist.Exists(b => b.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase) 
                                && b.Title.Equals(songTitle, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            _irc.SendPublicChatMessage($"This song is already on the blacklist @{_botConfig.Broadcaster}");
                            return;
                        }
                        else if (blacklist.Exists(b => b.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            _irc.SendPublicChatMessage($"This song's artist is already on the blacklist @{_botConfig.Broadcaster}");
                            return;
                        }
                    }

                    int recordsAffected = _songRequest.AddSongToBlacklist(songTitle, artist, _broadcasterId);

                    if (recordsAffected > 0)
                        _irc.SendPublicChatMessage($"The song \"{songTitle} by {artist}\" has been added to the blacklist @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"I'm sorry. I'm not able to add this song to the blacklist at this time @{_botConfig.Broadcaster}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Please insert request type (1 = artist/2 = song) @{_botConfig.Broadcaster}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdAddSongRequestBlacklist(string)", false, "!srbl");
            }
        }

        public void CmdRemoveSongRequestBlacklist(string message)
        {
            try
            {
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request @{_botConfig.Broadcaster}");
                    return;
                }

                string requestType = message.Substring(message.IndexOf(" ") + 1, 1);
                string request = message.Substring(requestIndex + 1);

                // Check if request is based on an artist or just a song by an artist
                if (requestType.Equals("1")) // remove blackout for any song by this artist
                {
                    // remove artist from db
                    int recordsAffected = _songRequest.DeleteArtistFromBlacklist(request, _broadcasterId);

                    if (recordsAffected > 0)
                        _irc.SendPublicChatMessage($"The artist \"{request}\" can now be requested @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"Couldn't find the requested artist for blacklist-removal @{_botConfig.Broadcaster}");
                }
                else if (requestType.Equals("2")) // remove blackout for a song by an artist
                {
                    if (request.Count(c => c == '"') < 2
                        || request.Count(c => c == '<') != 1
                        || request.Count(c => c == '>') != 1)
                    {
                        _irc.SendPublicChatMessage($"Please surround the song title with \" (quotation marks) " 
                            + $"and the artist with \"<\" and \">\" @{_botConfig.Broadcaster}");
                        return;
                    }

                    int songTitleStartIndex = message.IndexOf('"');
                    int songTitleEndIndex = message.LastIndexOf('"');
                    int artistStartIndex = message.IndexOf('<');
                    int artistEndIndex = message.IndexOf('>');

                    string songTitle = message.Substring(songTitleStartIndex + 1, songTitleEndIndex - songTitleStartIndex - 1);
                    string artist = message.Substring(artistStartIndex + 1, artistEndIndex - artistStartIndex - 1);

                    int recordsAffected = _songRequest.DeleteSongFromBlacklist(songTitle, artist, _broadcasterId);

                    if (recordsAffected > 0)
                        _irc.SendPublicChatMessage($"The song \"{songTitle} by {artist}\" can now requested @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"Couldn't find the requested song for blacklist-removal @{_botConfig.Broadcaster}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Please insert request type (1 = artist/2 = song) @{_botConfig.Broadcaster}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRemoveSongRequestBlacklist(string)", false, "!removesrbl");
            }
        }

        public void CmdResetSongRequestBlacklist()
        {
            try
            {
                int recordsAffected = _songRequest.ResetBlacklist(_broadcasterId);

                if (recordsAffected > 0)
                    _irc.SendPublicChatMessage($"Song Request Blacklist has been reset @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Song Request Blacklist is empty @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdResetSongRequestBlacklist()", false, "!resetsrbl");
            }
        }

        public void CmdListSongRequestBlacklist()
        {
            try
            {
                List<SongRequestBlacklistItem> blacklist = _songRequest.GetSongRequestBlackList(_broadcasterId);

                if (blacklist.Count == 0)
                {
                    _irc.SendPublicChatMessage($"The song request blacklist is empty @{_botConfig.Broadcaster}");
                    return;
                }

                string songList = "";

                foreach (SongRequestBlacklistItem item in blacklist.OrderBy(i => i.Artist))
                {
                    if (!string.IsNullOrEmpty(item.Title))
                        songList += $"\"{item.Title}\" - ";

                    songList += $"{item.Artist} >< ";
                }

                StringBuilder strBdrSongList = new StringBuilder(songList);
                strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " >< "
                songList = strBdrSongList.ToString(); // replace old song list string with new

                _irc.SendPublicChatMessage("Song Request Blacklist: < " + songList + " >");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdListSongRequestBlacklist()", false, "!showsrbl");
            }
        }

        public async Task CmdLive(bool hasTwitterInfo)
        {
            try
            {
                RootStreamJSON streamJSON = await _twitchInfo.GetBroadcasterStream();

                if (streamJSON.Stream == null)
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
                else if (!_botConfig.EnableTweets)
                    _irc.SendPublicChatMessage("Tweets are disabled at the moment");
                else if (_botConfig.EnableTweets && hasTwitterInfo)
                {
                    string tweetResult = _twitter.SendTweet($"Live on Twitch playing {streamJSON.Stream.Game} "
                        + $"\"{streamJSON.Stream.Channel.Status}\" twitch.tv/{_botConfig.Broadcaster}");

                    _irc.SendPublicChatMessage($"{tweetResult} @{_botConfig.Broadcaster}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdLive(bool)", false, "!live");
            }
        }

        public void CmdRefreshReminders()
        {
            try
            {
                Threads.ChatReminder.RefreshReminders();
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRefreshReminders()", false, "!refreshreminders");
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
                int gameId = _gameDirectory.GetGameId(gameTitle, out bool hasMultiplayer);

                // During refresh, make sure no fighters can join
                _bossFightSettingsInstance.RefreshBossFight = true;
                _bossFightSettingsInstance.LoadSettings(_broadcasterId, _connStr, gameId);
                _bossFightSettingsInstance.RefreshBossFight = false;

                _irc.SendPublicChatMessage($"Boss fight settings refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRefreshBossFight()", false, "!refreshbossfight");
            }
        }

        public void CmdSetRegularFollowerHours(string message)
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
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdSetRegularHours(string)", false, "!setregularhours");
            }
        }
    }
}
