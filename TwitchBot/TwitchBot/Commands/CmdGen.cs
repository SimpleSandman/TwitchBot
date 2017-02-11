using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using SpotifyAPI.Local.Models;
using System.Net.Http;
using Newtonsoft.Json;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

namespace TwitchBot.Commands
{
    public class CmdGen
    {
        private IrcClient _irc;
        private LocalSpotifyClient _spotify;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _intBroadcasterID;
        private TwitchInfoService _twitchInfo;
        private BankService _bank;
        private string _strBroadcasterGame;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public CmdGen(IrcClient irc, LocalSpotifyClient spotify, TwitchBotConfigurationSection botConfig, string connString, int broadcasterId, TwitchInfoService twitchInfo, BankService bank)
        {
            _irc = irc;
            _spotify = spotify;
            _botConfig = botConfig;
            _connStr = connString;
            _intBroadcasterID = broadcasterId;
            _twitchInfo = twitchInfo;
            _bank = bank;
        }

        public void CmdCmds()
        {
            try
            {
                _irc.sendPublicChatMessage("---> !hello >< !slap @[username] >< !stab @[username] >< !throw [item] @[username] >< !shoot @[username] "
                    + ">< !spotifycurr >< !srlist >< !sr [artist] - [song title] >< !utctime >< !hosttime >< !partyup [party member name] >< !gamble [money] "
                    + ">< !quote >< !myfunds <---"
                    + " Link to full list of commands: http://bit.ly/2bXLlEe");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdCmds()", false, "!cmds");
            }
        }

        public void CmdHello(string strUserName)
        {
            try
            {
                _irc.sendPublicChatMessage($"Hey @{strUserName}! Thanks for talking to me.");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdHello(string)", false, "!hello");
            }
        }

        public void CmdUtcTime()
        {
            try
            {
                _irc.sendPublicChatMessage($"UTC Time: {DateTime.UtcNow.ToString()}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdUtcTime()", false, "!utctime");
            }
        }

        public void CmdHostTime(string strBroadcasterName)
        {
            try
            {
                _irc.sendPublicChatMessage($"{strBroadcasterName}'s Current Time: {DateTime.Now.ToString()} ({TimeZone.CurrentTimeZone.StandardName})");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdHostTime(string)", false, "!hosttime");
            }
        }

        public void CmdDuration()
        {
            try
            {
                // Check if the channel is live
                if (TaskJSON.GetStream(_botConfig.Broadcaster, _botConfig.TwitchClientId).Result.stream != null)
                {
                    string strDuration = TaskJSON.GetStream(_botConfig.Broadcaster, _botConfig.TwitchClientId).Result.stream.created_at;
                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(strDuration, new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal);
                    string strResultDuration = String.Format("{0:h\\:mm\\:ss}", ts);
                    _irc.sendPublicChatMessage("This channel's current uptime (length of current stream) is " + strResultDuration);
                }
                else
                    _irc.sendPublicChatMessage("This channel is not streaming anything at the moment");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdDuration(string)", false, "!duration");
            }
        }

        /// <summary>
        /// Display list of requested songs
        /// </summary>
        public void CmdListSR()
        {
            try
            {
                string songList = "";

                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                DataTable schemaTable = reader.GetSchemaTable();
                                DataTable data = new DataTable();
                                foreach (DataRow row in schemaTable.Rows)
                                {
                                    string colName = row.Field<string>("ColumnName");
                                    Type t = row.Field<Type>("DataType");
                                    data.Columns.Add(colName, t);
                                }
                                while (reader.Read())
                                {
                                    var newRow = data.Rows.Add();
                                    foreach (DataColumn col in data.Columns)
                                    {
                                        newRow[col.ColumnName] = reader[col.ColumnName];
                                        Console.WriteLine(newRow[col.ColumnName].ToString());
                                        songList = songList + newRow[col.ColumnName].ToString() + " >< ";
                                    }
                                }
                                StringBuilder strBdrSongList = new StringBuilder(songList);
                                strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " >< "
                                songList = strBdrSongList.ToString(); // replace old song list string with new
                                _irc.sendPublicChatMessage("Current List of Requested Songs: " + songList);
                            }
                            else
                            {
                                Console.WriteLine("No requests have been made");
                                _irc.sendPublicChatMessage("No requests have been made");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSRList()", false, "!srlist");
            }
        }

        /// <summary>
        /// Request a song for the host to play
        /// </summary>
        /// <param name="isSongRequestAvail">Check if song request system is enabled</param>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdSR(bool isSongRequestAvail, string message, string strUserName)
        {
            try
            {
                // Check if song request system is enabled
                if (isSongRequestAvail)
                {
                    // Grab the song name from the request
                    int index = message.IndexOf("!sr");
                    string songRequest = message.Substring(index, message.Length - index);
                    songRequest = songRequest.Replace("!sr ", "");
                    Console.WriteLine("New song request: " + songRequest);

                    // Check if song request has more than letters, numbers, and hyphens
                    if (!Regex.IsMatch(songRequest, @"^[a-zA-Z0-9 \-\(\)\'\?]+$"))
                    {
                        _irc.sendPublicChatMessage("Only letters, numbers, hyphens (-), parentheses (), "
                            + "apostrophes ('), and question marks (?) are allowed. Please try again. "
                            + "If the problem persists, please contact my creator");
                    }
                    else
                    {
                        /* Add song request to database */
                        string query = "INSERT INTO tblSongRequests (songRequests, broadcaster, chatter) VALUES (@song, @broadcaster, @chatter)";

                        using (SqlConnection conn = new SqlConnection(_connStr))
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.Add("@song", SqlDbType.VarChar, 200).Value = songRequest;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.VarChar, 200).Value = _intBroadcasterID;
                            cmd.Parameters.Add("@chatter", SqlDbType.VarChar, 200).Value = strUserName;

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }

                        _irc.sendPublicChatMessage("The song \"" + songRequest + "\" has been successfully requested!");
                    }
                }
                else
                    _irc.sendPublicChatMessage("Song requests are disabled at the moment");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSR(bool, string, string)", false, "!sr", message);
            }
        }

        /// <summary>
        /// Displays the current song being played from Spotify
        /// </summary>
        public void CmdSpotifyCurr()
        {
            try
            {
                StatusResponse status = _spotify.GetStatus();
                if (status != null)
                {
                    _irc.sendPublicChatMessage("Current Song: " + status.Track.TrackResource.Name
                        + " >< Artist: " + status.Track.ArtistResource.Name
                        + " >< Album: " + status.Track.AlbumResource.Name);
                }
                else
                    _irc.sendPublicChatMessage("The broadcaster is not playing a song at the moment");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSpotifyCurr()", false, "!spotifycurr");
            }
        }

        /// <summary>
        /// Slaps a user and rates its effectiveness
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public async Task CmdSlap(string message, string strUserName)
        {
            try
            {
                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                await reactionCmd(strUserName, strRecipient, "Stop smacking yourself", "slaps", Effectiveness());
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSlap(string, string)", false, "!slap", message);
            }
        }

        /// <summary>
        /// Stabs a user and rates its effectiveness
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public async Task CmdStab(string message, string strUserName)
        {
            try
            {
                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                await reactionCmd(strUserName, strRecipient, "Stop stabbing yourself! You'll bleed out", "stabs", Effectiveness());
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdStab(string, string)", false, "!stab", message);
            }
        }

        /// <summary>
        /// Shoots a viewer's random body part
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public async Task CmdShoot(string message, string strUserName)
        {
            try
            {
                string strBodyPart = "'s ";
                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                Random rnd = new Random(DateTime.Now.Millisecond);
                int intBodyPart = rnd.Next(8); // between 0 and 7

                if (intBodyPart == 0)
                    strBodyPart += "head";
                else if (intBodyPart == 1)
                    strBodyPart += "left leg";
                else if (intBodyPart == 2)
                    strBodyPart += "right leg";
                else if (intBodyPart == 3)
                    strBodyPart += "left arm";
                else if (intBodyPart == 4)
                    strBodyPart += "right arm";
                else if (intBodyPart == 5)
                    strBodyPart += "stomach";
                else if (intBodyPart == 6)
                    strBodyPart += "neck";
                else // found largest random value
                    strBodyPart = " but missed";

                if (strBodyPart.Equals(" but missed"))
                {
                    _irc.sendPublicChatMessage("Ha! You missed @" + strUserName);
                }
                else
                {
                    // bot makes a special response if shot at
                    if (strRecipient.Equals(_botConfig.BotName.ToLower()))
                    {
                        _irc.sendPublicChatMessage("You think shooting me in the " + strBodyPart.Replace("'s ", "") + " would hurt me? I am a bot!");
                    }
                    else // viewer is the target
                    {
                        await reactionCmd(strUserName, strRecipient, "You just shot your " + strBodyPart.Replace("'s ", ""), "shoots", strBodyPart);
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdShoot(string, string)", false, "!shoot", message);
            }
        }

        /// <summary>
        /// Throws an item at a viewer and rates its effectiveness against the victim
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public async Task CmdThrow(string message, string strUserName)
        {
            try
            {
                int intIndexAction = 7;

                if (message.StartsWith("!throw @"))
                    _irc.sendPublicChatMessage("Please throw an item to a user @" + strUserName);
                else
                {
                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    string item = message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1);

                    await reactionCmd(strUserName, strRecipient, "Stop throwing " + item + " at yourself", "throws " + item + " at", ". " + Effectiveness());
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdThrow(string, string)", false, "!throw", message);
            }
        }

        /// <summary>
        /// Request party member if game and character exists in party up system
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdPartyUp(string message, string strUserName)
        {
            try
            {
                string strPartyMember = "";
                int intInputIndex = message.IndexOf(" ") + 1;
                int intGameID = 0;
                bool isPartyMemebrFound = false;
                bool isDuplicateRequestor = false;

                // Get current game
                _strBroadcasterGame = TaskJSON.GetChannel(_botConfig.Broadcaster, _botConfig.TwitchClientId).Result.game;

                // check if user entered something
                if (message.Length < intInputIndex)
                    _irc.sendPublicChatMessage("Please enter a party member @" + strUserName);
                else
                    strPartyMember = message.Substring(intInputIndex);

                // grab game id in order to find party member
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (_strBroadcasterGame.Equals(reader["name"].ToString()))
                                {
                                    intGameID = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }

                // if the game is not found
                // tell users this game is not accepting party up requests
                if (intGameID == 0)
                    _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else // check if user has already requested a party member
                {
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUpRequests "
                            + "WHERE broadcaster = @broadcaster AND game = @game AND username = @username", conn))
                        {
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                            cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strUserName;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (strUserName.ToLower().Equals(reader["username"].ToString().ToLower()))
                                        {
                                            isDuplicateRequestor = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (isDuplicateRequestor)
                        _irc.sendPublicChatMessage("You have already requested a party member. Please wait until your request has been completed @" + strUserName);
                    else // search for party member user is requesting
                    {
                        using (SqlConnection conn = new SqlConnection(_connStr))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUp WHERE broadcaster = @broadcaster AND game = @game", conn))
                            {
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (strPartyMember.ToLower().Equals(reader["partyMember"].ToString().ToLower()))
                                            {
                                                strPartyMember = reader["partyMember"].ToString();
                                                isPartyMemebrFound = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // insert party member if they exists from database
                        if (!isPartyMemebrFound)
                            _irc.sendPublicChatMessage("I couldn't find the requested party member '" + strPartyMember + "' @" + strUserName
                                + ". Please check with the broadcaster for possible spelling errors");
                        else
                        {
                            string query = "INSERT INTO tblPartyUpRequests (username, partyMember, timeRequested, broadcaster, game) "
                                + "VALUES (@username, @partyMember, @timeRequested, @broadcaster, @game)";

                            // Create connection and command
                            using (SqlConnection conn = new SqlConnection(_connStr))
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = strUserName;
                                cmd.Parameters.Add("@partyMember", SqlDbType.VarChar, 50).Value = strPartyMember;
                                cmd.Parameters.Add("@timeRequested", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                                conn.Close();
                            }

                            _irc.sendPublicChatMessage("@" + strUserName + ": " + strPartyMember + " has been added to the party queue");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdPartyUp(string, string)", false, "!partyup", message);
            }
        }

        /// <summary>
        /// Check what other user's have requested
        /// </summary>
        public void CmdPartyUpRequestList()
        {
            try
            {
                string strPartyList = "Here are the requested party members: ";
                int intGameID = 0;

                // Get current game
                _strBroadcasterGame = TaskJSON.GetChannel(_botConfig.Broadcaster, _botConfig.TwitchClientId).Result.game;

                // grab game id in order to find party member
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (_strBroadcasterGame.Equals(reader["name"].ToString()))
                                {
                                    intGameID = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }

                // if the game is not found
                // tell users this game is not part of the party up system
                if (intGameID == 0)
                    _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else
                {
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT username, partyMember FROM tblPartyUpRequests "
                            + "WHERE game = @game AND broadcaster = @broadcaster ORDER BY Id", conn))
                        {
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        strPartyList += reader["partyMember"].ToString() + " <-- " + reader["username"].ToString() + " // ";
                                    }
                                    StringBuilder strBdrPartyList = new StringBuilder(strPartyList);
                                    strBdrPartyList.Remove(strPartyList.Length - 4, 4); // remove extra " // "
                                    strPartyList = strBdrPartyList.ToString(); // replace old party member list string with new
                                    _irc.sendPublicChatMessage(strPartyList);
                                }
                                else
                                {
                                    Console.WriteLine("No party members are set for this game");
                                    _irc.sendPublicChatMessage("No party members are set for this game");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdPartyUpRequestList()", false, "!partyuprequestlist");
            }
        }

        /// <summary>
        /// Check what party members are available (if game is part of the party up system)
        /// </summary>
        public void CmdPartyUpList()
        {
            try
            {
                string strPartyList = "The available party members are: ";
                int intGameID = 0;

                // Get current game
                _strBroadcasterGame = TaskJSON.GetChannel(_botConfig.Broadcaster, _botConfig.TwitchClientId).Result.game;

                // grab game id in order to find party member
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (_strBroadcasterGame.Equals(reader["name"].ToString()))
                                {
                                    intGameID = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }

                // if the game is not found
                // tell users this game is not part of the party up system
                if (intGameID == 0)
                    _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else
                {
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT partyMember FROM tblPartyUp WHERE game = @game AND broadcaster = @broadcaster ORDER BY partyMember", conn))
                        {
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        strPartyList += reader["partyMember"].ToString() + " >< ";
                                    }
                                    StringBuilder strBdrPartyList = new StringBuilder(strPartyList);
                                    strBdrPartyList.Remove(strPartyList.Length - 4, 4); // remove extra " >< "
                                    strPartyList = strBdrPartyList.ToString(); // replace old party member list string with new
                                    _irc.sendPublicChatMessage(strPartyList);
                                }
                                else
                                {
                                    Console.WriteLine("No party members are set for this game");
                                    _irc.sendPublicChatMessage("No party members are set for this game");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdPartyUpList()", false, "!partyuplist");
            }
        }

        /// <summary>
        /// Check user's account balance
        /// </summary>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdCheckFunds(string strUserName)
        {
            try
            {
                int intBalance = _bank.CheckBalance(strUserName, _intBroadcasterID);

                if (intBalance == -1)
                    _irc.sendPublicChatMessage("You are not currently banking with us at the moment. Please talk to a moderator about acquiring " + _botConfig.CurrencyType);
                else
                    _irc.sendPublicChatMessage("@" + strUserName + " currently has " + intBalance.ToString() + " " + _botConfig.CurrencyType);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdCheckFunds()", false, "!myfunds");
            }
        }

        /// <summary>
        /// Gamble away currency
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdGamble(string message, string strUserName)
        {
            try
            {
                int intGambledMoney = 0; // Money put into the gambling system
                bool bolValid = int.TryParse(message.Substring(message.IndexOf(" ") + 1), out intGambledMoney);
                int intWalletBalance = _bank.CheckBalance(strUserName, _intBroadcasterID);

                if (!bolValid || intGambledMoney < 1)
                    _irc.sendPublicChatMessage($"Please insert a positive whole amount (no decimal numbers) to gamble @{strUserName}");
                else if (intGambledMoney > intWalletBalance)
                    _irc.sendPublicChatMessage($"You do not have the sufficient funds to gamble {intGambledMoney} {_botConfig.CurrencyType} @{strUserName}");
                else
                {
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int intDiceRoll = rnd.Next(1, 101); // between 1 and 100
                    int intNewBalance = 0;

                    string strResult = $"Gambled {intGambledMoney} {_botConfig.CurrencyType} and the dice roll was {intDiceRoll}. @{strUserName} ";

                    // Check the 100-sided die roll result
                    if (intDiceRoll < 61) // lose gambled money
                    {
                        intNewBalance = intWalletBalance - intGambledMoney;
                        _bank.UpdateFunds(strUserName, _intBroadcasterID, intNewBalance);
                        strResult += $"lost {intGambledMoney} {_botConfig.CurrencyType}";
                    }
                    else if (intDiceRoll >= 61 && intDiceRoll <= 98) // earn double
                    {
                        intWalletBalance -= intGambledMoney; // put money into the gambling pot (remove money from wallet)
                        intNewBalance = intWalletBalance + (intGambledMoney * 2); // recieve 2x earnings back into wallet
                        _bank.UpdateFunds(strUserName, _intBroadcasterID, intNewBalance);
                        strResult += $"won {intGambledMoney * 2} {_botConfig.CurrencyType}";
                    }
                    else if (intDiceRoll == 99 || intDiceRoll == 100) // earn triple
                    {
                        intWalletBalance -= intGambledMoney; // put money into the gambling pot (remove money from wallet)
                        intNewBalance = intWalletBalance + (intGambledMoney * 3); // recieve 3x earnings back into wallet
                        _bank.UpdateFunds(strUserName, _intBroadcasterID, intNewBalance);
                        strResult += $"won {intGambledMoney * 3} {_botConfig.CurrencyType}";
                    }

                    strResult += $" and now has {intNewBalance} {_botConfig.CurrencyType}";

                    _irc.sendPublicChatMessage(strResult);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdGamble(string, string)", false, "!gamble", message);
            }
        }

        /// <summary>
        /// Display random broadcaster quote
        /// </summary>
        public void CmdQuote()
        {
            try
            {
                List<Quote> lstQuote = new List<Quote>();

                // Get quotes from tblQuote and put them into a list
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblQuote WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Quote qte = new Quote();
                                    qte.strMessage = reader["userQuote"].ToString();
                                    qte.strAuthor = reader["username"].ToString();
                                    qte.dtTimeCreated = Convert.ToDateTime(reader["timeCreated"]);
                                    lstQuote.Add(qte);
                                }
                            }
                        }
                    }
                }

                // Check if there any quotes inside the system
                if (lstQuote.Count == 0)
                    _irc.sendPublicChatMessage("There are no quotes to be displayed at the moment");
                else
                {
                    // Randomly pick a quote from the list to display
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int intIndex = rnd.Next(lstQuote.Count);

                    Quote qteResult = new Quote();
                    qteResult = lstQuote.ElementAt(intIndex); // grab random quote from list of quotes
                    string strQuote = $"\"{qteResult.strMessage}\" - {_botConfig.Broadcaster} " +
                        $"({qteResult.dtTimeCreated.ToString("MMMM", CultureInfo.InvariantCulture)} {qteResult.dtTimeCreated.Year})";

                    _irc.sendPublicChatMessage(strQuote);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdQuote()", false, "!quote");
            }
        }

        /// <summary>
        /// Tell the user how long they have been following the broadcaster
        /// </summary>
        /// <param name="strUserName"></param>
        public async Task CmdFollowSince(string strUserName)
        {
            try
            {
                using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(strUserName))
                {
                    if (message.IsSuccessStatusCode)
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        FollowingSinceJSON response = JsonConvert.DeserializeObject<FollowingSinceJSON>(body);
                        DateTime startedFollowing = Convert.ToDateTime(response.created_at);
                        //TimeSpan howLong = DateTime.Now - startedFollowing;
                        _irc.sendPublicChatMessage($"@{strUserName} has been following since {startedFollowing.ToLongDateString()}");
                    }
                    else
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        ErrMsgJSON response = JsonConvert.DeserializeObject<ErrMsgJSON>(body);
                        _irc.sendPublicChatMessage(response.message);
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdFollowSince()", false, "!followsince");
            }
        }

        /// <summary>
        /// Display the follower's stream rank
        /// </summary>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public async Task CmdViewRank(string strUserName)
        {
            try
            {
                using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(strUserName))
                {
                    if (message.IsSuccessStatusCode)
                    {
                        int currExp = -1;

                        // find existing follower's experience points (if available)
                        using (SqlConnection conn = new SqlConnection(_connStr))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblRankFollowers WHERE broadcaster = @broadcaster", conn))
                            {
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (strUserName.Equals(reader["username"].ToString()))
                                            {
                                                currExp = int.Parse(reader["exp"].ToString());
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Grab the follower's associated rank
                        if (currExp > -1)
                        {
                            List<Rank> lstRanks = new List<Rank>();
                            Rank currFollowerRank = new Rank();

                            // get list of ranks currently for the specific broadcaster
                            using (SqlConnection conn = new SqlConnection(_connStr))
                            {
                                conn.Open();
                                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblRank WHERE broadcaster = @broadcaster", conn))
                                {
                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            while (reader.Read())
                                            {
                                                Rank rank = new Rank()
                                                {
                                                    Name = reader["name"].ToString(),
                                                    ExpCap = int.Parse(reader["expCap"].ToString())
                                                };
                                                lstRanks.Add(rank);
                                            }
                                        }
                                    }
                                }
                            }

                            // find the user's current rank by experience cap
                            foreach (var followerRank in lstRanks.OrderBy(r => r.ExpCap))
                            {
                                // search until current experience < experience cap
                                if (currExp >= followerRank.ExpCap)
                                {
                                    continue;
                                }
                                else
                                {
                                    currFollowerRank.Name = followerRank.Name;
                                    currFollowerRank.ExpCap = followerRank.ExpCap;
                                    break;
                                }
                            }

                            decimal hoursWatched = Math.Round(Convert.ToDecimal(currExp) / (decimal)12.0, 2);

                            _irc.sendPublicChatMessage($"@{strUserName}: \"{currFollowerRank.Name}\" {currExp}/{currFollowerRank.ExpCap} EXP ({hoursWatched} hours)");
                        }
                        else
                        {
                            // Add follower to the ranks (mainly for existing followers without a rank)
                            string query = "INSERT INTO tblRankFollowers (username, exp, broadcaster) "
                                    + "VALUES (@username, @exp, @broadcaster)";

                            using (SqlConnection conn = new SqlConnection(_connStr))
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strUserName;
                                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = 0; // initial experience
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }

                            _irc.sendPublicChatMessage($"Welcome to the army @{strUserName}. View your new rank using !rank");
                        }
                    }
                    else
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        ErrMsgJSON response = JsonConvert.DeserializeObject<ErrMsgJSON>(body);
                        _irc.sendPublicChatMessage(response.message);
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdViewRank()", false, "!rank");
            }
        }

        private async Task<bool> reactionCmd(string strOrigUser, string strRecipient, string strMsgToSelf, string strAction, string strAddlMsg = "")
        {
            // check if user is trying to use a command on themselves
            if (strOrigUser.Equals(strRecipient))
            {
                _irc.sendPublicChatMessage(strMsgToSelf + " @" + strOrigUser);
                return true;
            }

            // check if user currently watching the channel
            if (await chatterValid(strOrigUser, strRecipient))
            {
                _irc.sendPublicChatMessage(strOrigUser + " " + strAction + " @" + strRecipient + " " + strAddlMsg);
                return true;
            }
            else
                return false;
        }

        private async Task<bool> chatterValid(string strOrigUser, string strRecipient)
        {
            // Check if the requested user is this bot
            if (strRecipient.Equals(_botConfig.BotName.ToLower()))
                return true;

            // Grab user's chatter info (viewers, mods, etc.)
            List<List<string>> lstAvailChatterType = await _twitchInfo.GetChatterListByType();
            if (lstAvailChatterType.Count > 0)
            {
                // Search for user
                for (int i = 0; i < lstAvailChatterType.Count(); i++)
                {
                    foreach (string chatter in lstAvailChatterType[i])
                    {
                        if (chatter.Equals(strRecipient.ToLower()))
                            return true;
                    }
                }
            }

            // finished searching with no results
            _irc.sendPublicChatMessage("@" + strOrigUser + ": I cannot find the user you wanted to interact with. Perhaps the user left us?");
            return false;
        }

        private string Effectiveness()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int intEffectiveLvl = rnd.Next(3); // between 0 and 2
            string strEffectiveness = "";

            if (intEffectiveLvl == 0)
                strEffectiveness = "It's super effective!";
            else if (intEffectiveLvl == 1)
                strEffectiveness = "It wasn't very effective";
            else
                strEffectiveness = "It had no effect";

            return strEffectiveness;
        }
    }
}
