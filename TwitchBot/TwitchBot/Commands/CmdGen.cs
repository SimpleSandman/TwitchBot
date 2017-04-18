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

using Google.Apis.YouTube.v3.Data;

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
        private int _broadcasterId;
        private TwitchInfoService _twitchInfo;
        private BankService _bank;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;

        public CmdGen(IrcClient irc, LocalSpotifyClient spotify, TwitchBotConfigurationSection botConfig, string connString, int broadcasterId, TwitchInfoService twitchInfo, BankService bank)
        {
            _irc = irc;
            _spotify = spotify;
            _botConfig = botConfig;
            _connStr = connString;
            _broadcasterId = broadcasterId;
            _twitchInfo = twitchInfo;
            _bank = bank;
        }

        public void CmdDisplayCmds()
        {
            try
            {
                _irc.SendPublicChatMessage("---> !hello >< !slap @[username] >< !stab @[username] >< !throw [item] @[username] >< !shoot @[username] "
                    + ">< !sr [youtube link/search] >< !sl >< !partyup [party member name] >< !gamble [money] "
                    + ">< !quote >< !" + _botConfig.CurrencyType.ToLower() + " (check stream currency) <---"
                    + " Link to full list of commands: http://bit.ly/2bXLlEe");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdCmds()", false, "!cmds");
            }
        }

        public void CmdHello(string username)
        {
            try
            {
                _irc.SendPublicChatMessage($"Hey @{username}! Thanks for talking to me.");
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
                _irc.SendPublicChatMessage($"UTC Time: {DateTime.UtcNow.ToString()}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdUtcTime()", false, "!utctime");
            }
        }

        public void CmdHostTime()
        {
            try
            {
                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster}'s Current Time: {DateTime.Now.ToString()} ({TimeZone.CurrentTimeZone.StandardName})");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdHostTime()", false, "!hosttime");
            }
        }

        public async Task CmdUptime()
        {
            try
            {
                RootStreamJSON streamJson = await TaskJSON.GetStream(_botConfig.Broadcaster, _botConfig.TwitchClientId);

                // Check if the channel is live
                if (streamJson.stream != null)
                {
                    string duration = streamJson.stream.created_at;
                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(duration, new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal);
                    string strResultDuration = String.Format("{0:h\\:mm\\:ss}", ts);
                    _irc.SendPublicChatMessage("This channel's current uptime (length of current stream) is " + strResultDuration);
                }
                else
                    _irc.SendPublicChatMessage("This channel is not streaming anything at the moment");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdUptime()", false, "!uptime");
            }
        }

        /// <summary>
        /// Display list of requested songs
        /// </summary>
        public void CmdListManualSr()
        {
            try
            {
                string songList = "";

                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
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
                                        Console.WriteLine(newRow[col.ColumnName]);
                                        songList = songList + newRow[col.ColumnName] + " >< ";
                                    }
                                }
                                StringBuilder strBdrSongList = new StringBuilder(songList);
                                strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " >< "
                                songList = strBdrSongList.ToString(); // replace old song list string with new
                                _irc.SendPublicChatMessage("Current List of Requested Songs: " + songList);
                            }
                            else
                            {
                                Console.WriteLine("No requests have been made");
                                _irc.SendPublicChatMessage("No requests have been made");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdListManualSr()", false, "!rbsrlist");
            }
        }

        /// <summary>
        /// Request a song for the host to play
        /// </summary>
        /// <param name="isSongRequestAvail">Check if song request system is enabled</param>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public void CmdManualSr(bool isSongRequestAvail, string message, string username)
        {
            try
            {
                // Check if song request system is enabled
                if (isSongRequestAvail)
                {
                    // Grab the song name from the request
                    int index = message.IndexOf(" ");
                    string songRequest = message.Substring(index + 1);
                    Console.WriteLine("New song request: " + songRequest);

                    // Check if song request has more than allowed symbols
                    if (!Regex.IsMatch(songRequest, @"^[a-zA-Z0-9 \-\(\)\'\?\,\/\""]+$"))
                    {
                        _irc.SendPublicChatMessage("Only letters, numbers, commas, hyphens, parentheses, "
                            + "apostrophes, forward-slash, and question marks are allowed. Please try again. "
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
                            cmd.Parameters.Add("@broadcaster", SqlDbType.VarChar, 200).Value = _broadcasterId;
                            cmd.Parameters.Add("@chatter", SqlDbType.VarChar, 200).Value = username;

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                        _irc.SendPublicChatMessage("The song \"" + songRequest + "\" has been successfully requested!");
                    }
                }
                else
                    _irc.SendPublicChatMessage("Song requests are disabled at the moment");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdManualSr(bool, string, string)", false, "!rbsr", message);
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
                    _irc.SendPublicChatMessage("Current Song: " + status.Track.TrackResource.Name
                        + " >< Artist: " + status.Track.ArtistResource.Name
                        + " >< Album: " + status.Track.AlbumResource.Name);
                }
                else
                    _irc.SendPublicChatMessage("The broadcaster is not playing a song at the moment");
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
        /// <param name="username">User that sent the message</param>
        public async Task CmdSlap(string message, string username)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                await ReactionCmd(username, recipient, "Stop smacking yourself", "slaps", Effectiveness());
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
        /// <param name="username">User that sent the message</param>
        public async Task CmdStab(string message, string username)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                await ReactionCmd(username, recipient, "Stop stabbing yourself! You'll bleed out", "stabs", Effectiveness());
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
        /// <param name="username">User that sent the message</param>
        public async Task CmdShoot(string message, string username)
        {
            try
            {
                string bodyPart = "'s ";
                string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                Random rnd = new Random(DateTime.Now.Millisecond);
                int bodyPartId = rnd.Next(8); // between 0 and 7

                if (bodyPartId == 0)
                    bodyPart += "head";
                else if (bodyPartId == 1)
                    bodyPart += "left leg";
                else if (bodyPartId == 2)
                    bodyPart += "right leg";
                else if (bodyPartId == 3)
                    bodyPart += "left arm";
                else if (bodyPartId == 4)
                    bodyPart += "right arm";
                else if (bodyPartId == 5)
                    bodyPart += "stomach";
                else if (bodyPartId == 6)
                    bodyPart += "neck";
                else // found largest random value
                    bodyPart = " but missed";

                if (bodyPart.Equals(" but missed"))
                {
                    _irc.SendPublicChatMessage("Ha! You missed @" + username);
                }
                else
                {
                    // bot makes a special response if shot at
                    if (recipient.Equals(_botConfig.BotName.ToLower()))
                    {
                        _irc.SendPublicChatMessage("You think shooting me in the " + bodyPart.Replace("'s ", "") + " would hurt me? I am a bot!");
                    }
                    else // viewer is the target
                    {
                        await ReactionCmd(username, recipient, "You just shot your " + bodyPart.Replace("'s ", ""), "shoots", bodyPart);
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
        /// <param name="username">User that sent the message</param>
        public async Task CmdThrow(string message, string username)
        {
            try
            {
                int indexAction = 7;

                if (message.StartsWith("!throw @"))
                    _irc.SendPublicChatMessage("Please throw an item to a user @" + username);
                else
                {
                    string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    string item = message.Substring(indexAction, message.IndexOf("@") - indexAction - 1);

                    await ReactionCmd(username, recipient, "Stop throwing " + item + " at yourself", "throws " + item + " at", ". " + Effectiveness());
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
        /// <param name="username">User that sent the message</param>
        public async Task CmdPartyUp(string message, string username)
        {
            try
            {
                string partyMember = "";
                int inputIndex = message.IndexOf(" ") + 1;
                int gameId = 0;
                bool isPartyMemebrFound = false;
                bool isDuplicateRequestor = false;

                // Get current game
                ChannelJSON json = await TaskJSON.GetChannel(_botConfig.Broadcaster, _botConfig.TwitchClientId);
                string broadcasterGame = json.game;

                // check if user entered something
                if (message.Length < inputIndex)
                    _irc.SendPublicChatMessage("Please enter a party member @" + username);
                else
                    partyMember = message.Substring(inputIndex);

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
                                if (broadcasterGame.Equals(reader["name"].ToString()))
                                {
                                    gameId = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }

                // if the game is not found
                // tell users this game is not accepting party up requests
                if (gameId == 0)
                    _irc.SendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else // check if user has already requested a party member
                {
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUpRequests "
                            + "WHERE broadcaster = @broadcaster AND game = @game AND username = @username", conn))
                        {
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                            cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = username;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (username.ToLower().Equals(reader["username"].ToString().ToLower()))
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
                        _irc.SendPublicChatMessage("You have already requested a party member. Please wait until your request has been completed @" + username);
                    else // search for party member user is requesting
                    {
                        using (SqlConnection conn = new SqlConnection(_connStr))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUp WHERE broadcaster = @broadcaster AND game = @game", conn))
                            {
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (partyMember.ToLower().Equals(reader["partyMember"].ToString().ToLower()))
                                            {
                                                partyMember = reader["partyMember"].ToString();
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
                            _irc.SendPublicChatMessage("I couldn't find the requested party member '" + partyMember + "' @" + username
                                + ". Please check with the broadcaster for possible spelling errors");
                        else
                        {
                            string query = "INSERT INTO tblPartyUpRequests (username, partyMember, timeRequested, broadcaster, game) "
                                + "VALUES (@username, @partyMember, @timeRequested, @broadcaster, @game)";

                            // Create connection and command
                            using (SqlConnection conn = new SqlConnection(_connStr))
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;
                                cmd.Parameters.Add("@partyMember", SqlDbType.VarChar, 50).Value = partyMember;
                                cmd.Parameters.Add("@timeRequested", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                                conn.Close();
                            }

                            _irc.SendPublicChatMessage("@" + username + ": " + partyMember + " has been added to the party queue");
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
        public async Task CmdPartyUpRequestList()
        {
            try
            {
                string partyList = "Here are the requested party members: ";
                int gameId = 0;

                // Get current game
                ChannelJSON json = await TaskJSON.GetChannel(_botConfig.Broadcaster, _botConfig.TwitchClientId);
                string broadcasterGame = json.game;

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
                                if (broadcasterGame.Equals(reader["name"].ToString()))
                                {
                                    gameId = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }

                // if the game is not found
                // tell users this game is not part of the party up system
                if (gameId == 0)
                    _irc.SendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else
                {
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT username, partyMember FROM tblPartyUpRequests "
                            + "WHERE game = @game AND broadcaster = @broadcaster ORDER BY Id", conn))
                        {
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        partyList += reader["partyMember"].ToString() + " <-- " + reader["username"].ToString() + " // ";
                                    }
                                    StringBuilder modPartyListMsg = new StringBuilder(partyList);
                                    modPartyListMsg.Remove(partyList.Length - 4, 4); // remove extra " // "
                                    partyList = modPartyListMsg.ToString(); // replace old party member list string with new
                                    _irc.SendPublicChatMessage(partyList);
                                }
                                else
                                {
                                    Console.WriteLine("No party members are set for this game");
                                    _irc.SendPublicChatMessage("No party members are set for this game");
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
        public async Task CmdPartyUpList()
        {
            try
            {
                string partyList = "The available party members are: ";
                int gameId = 0;

                // Get current game
                ChannelJSON json = await TaskJSON.GetChannel(_botConfig.Broadcaster, _botConfig.TwitchClientId);
                string broadcasterGame = json.game;

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
                                if (broadcasterGame.Equals(reader["name"].ToString()))
                                {
                                    gameId = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }

                // if the game is not found
                // tell users this game is not part of the party up system
                if (gameId == 0)
                    _irc.SendPublicChatMessage("This game is currently not a part of the \"Party Up\" system");
                else
                {
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT partyMember FROM tblPartyUp WHERE game = @game AND broadcaster = @broadcaster ORDER BY partyMember", conn))
                        {
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        partyList += reader["partyMember"].ToString() + " >< ";
                                    }
                                    StringBuilder modPartyListMsg = new StringBuilder(partyList);
                                    modPartyListMsg.Remove(partyList.Length - 4, 4); // remove extra " >< "
                                    partyList = modPartyListMsg.ToString(); // replace old party member list string with new
                                    _irc.SendPublicChatMessage(partyList);
                                }
                                else
                                {
                                    Console.WriteLine("No party members are set for this game");
                                    _irc.SendPublicChatMessage("No party members are set for this game");
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
        /// <param name="username">User that sent the message</param>
        public void CmdCheckFunds(string username)
        {
            try
            {
                int balance = _bank.CheckBalance(username, _broadcasterId);

                if (balance == -1)
                    _irc.SendPublicChatMessage("You are not currently banking with us at the moment. Please talk to a moderator about acquiring " + _botConfig.CurrencyType);
                else
                    _irc.SendPublicChatMessage("@" + username + " currently has " + balance.ToString() + " " + _botConfig.CurrencyType);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdCheckFunds(string)", false, "![currency name]");
            }
        }

        /// <summary>
        /// Gamble away currency
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public void CmdGamble(string message, string username)
        {
            try
            {
                int gambledMoney = 0; // Money put into the gambling system
                bool isValidMsg = int.TryParse(message.Substring(message.IndexOf(" ") + 1), out gambledMoney);
                int walletBalance = _bank.CheckBalance(username, _broadcasterId);

                if (!isValidMsg || gambledMoney < 1)
                    _irc.SendPublicChatMessage($"Please insert a positive whole amount (no decimal numbers) to gamble @{username}");
                else if (gambledMoney > walletBalance)
                    _irc.SendPublicChatMessage($"You do not have the sufficient funds to gamble {gambledMoney} {_botConfig.CurrencyType} @{username}");
                else
                {
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int diceRoll = rnd.Next(1, 101); // between 1 and 100
                    int newBalance = 0;

                    string result = $"@{username} gambled {gambledMoney} {_botConfig.CurrencyType} and the dice roll was {diceRoll}. They ";

                    // Check the 100-sided die roll result
                    if (diceRoll < 61) // lose gambled money
                    {
                        newBalance = walletBalance - gambledMoney;
                        _bank.UpdateFunds(username, _broadcasterId, newBalance);
                        result += $"lost {gambledMoney} {_botConfig.CurrencyType}";
                    }
                    else if (diceRoll >= 61 && diceRoll <= 98) // earn double
                    {
                        walletBalance -= gambledMoney; // put money into the gambling pot (remove money from wallet)
                        newBalance = walletBalance + (gambledMoney * 2); // recieve 2x earnings back into wallet
                        _bank.UpdateFunds(username, _broadcasterId, newBalance);
                        result += $"won {gambledMoney * 2} {_botConfig.CurrencyType}";
                    }
                    else if (diceRoll == 99 || diceRoll == 100) // earn triple
                    {
                        walletBalance -= gambledMoney; // put money into the gambling pot (remove money from wallet)
                        newBalance = walletBalance + (gambledMoney * 3); // recieve 3x earnings back into wallet
                        _bank.UpdateFunds(username, _broadcasterId, newBalance);
                        result += $"won {gambledMoney * 3} {_botConfig.CurrencyType}";
                    }

                    result += $" and now has {newBalance} {_botConfig.CurrencyType}";

                    _irc.SendPublicChatMessage(result);
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
                List<Quote> quoteList = new List<Quote>();

                // Get quotes from tblQuote and put them into a list
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblQuote WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Quote quote = new Quote();
                                    quote.Message = reader["userQuote"].ToString();
                                    quote.Author = reader["username"].ToString();
                                    quote.TimeCreated = Convert.ToDateTime(reader["timeCreated"]);
                                    quoteList.Add(quote);
                                }
                            }
                        }
                    }
                }

                // Check if there any quotes inside the system
                if (quoteList.Count == 0)
                    _irc.SendPublicChatMessage("There are no quotes to be displayed at the moment");
                else
                {
                    // Randomly pick a quote from the list to display
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int index = rnd.Next(quoteList.Count);

                    Quote resultingQuote = new Quote();
                    resultingQuote = quoteList.ElementAt(index); // grab random quote from list of quotes
                    string quoteResult = $"\"{resultingQuote.Message}\" - {_botConfig.Broadcaster} " +
                        $"({resultingQuote.TimeCreated.ToString("MMMM", CultureInfo.InvariantCulture)} {resultingQuote.TimeCreated.Year})";

                    _irc.SendPublicChatMessage(quoteResult);
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
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task CmdFollowSince(string username)
        {
            try
            {
                using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(username))
                {
                    if (message.IsSuccessStatusCode)
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        FollowingSinceJSON response = JsonConvert.DeserializeObject<FollowingSinceJSON>(body);
                        DateTime startedFollowing = Convert.ToDateTime(response.created_at);
                        //TimeSpan howLong = DateTime.Now - startedFollowing;
                        _irc.SendPublicChatMessage($"@{username} has been following since {startedFollowing.ToLongDateString()}");
                    }
                    else
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        ErrMsgJSON response = JsonConvert.DeserializeObject<ErrMsgJSON>(body);
                        _irc.SendPublicChatMessage(response.message);
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdFollowSince(string)", false, "!followsince");
            }
        }

        /// <summary>
        /// Display the follower's stream rank
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task CmdViewRank(string username)
        {
            try
            {
                using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(username))
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
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (username.Equals(reader["username"].ToString()))
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
                            List<Rank> ranksList = new List<Rank>();
                            Rank currFollowerRank = new Rank();

                            // get list of ranks currently for the specific broadcaster
                            using (SqlConnection conn = new SqlConnection(_connStr))
                            {
                                conn.Open();
                                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblRank WHERE broadcaster = @broadcaster", conn))
                                {
                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
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
                                                ranksList.Add(rank);
                                            }
                                        }
                                    }
                                }
                            }

                            // find the user's current rank by experience cap
                            foreach (var followerRank in ranksList.OrderBy(r => r.ExpCap))
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

                            _irc.SendPublicChatMessage($"@{username}: \"{currFollowerRank.Name}\" {currExp}/{currFollowerRank.ExpCap} EXP ({hoursWatched} hours)");
                        }
                        else
                        {
                            // Add follower to the ranks (mainly for existing followers without a rank)
                            string query = "INSERT INTO tblRankFollowers (username, exp, broadcaster) "
                                    + "VALUES (@username, @exp, @broadcaster)";

                            using (SqlConnection conn = new SqlConnection(_connStr))
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = username;
                                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = 0; // initial experience
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }

                            _irc.SendPublicChatMessage($"Welcome to the army @{username}. View your new rank using !rank");
                        }
                    }
                    else
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        ErrMsgJSON response = JsonConvert.DeserializeObject<ErrMsgJSON>(body);
                        _irc.SendPublicChatMessage(response.message);
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdViewRank(string)", false, "!rank");
            }
        }

        public async Task CmdYouTubeSongRequest(string message, string username, bool hasYouTubeAuth, bool isYouTubeSongRequestAvail)
        {
            try
            {
                if (!hasYouTubeAuth)
                {
                    _irc.SendPublicChatMessage("YouTube song requests have not been set up");
                }
                else if (!isYouTubeSongRequestAvail)
                {
                    _irc.SendPublicChatMessage("YouTube song requests are not turned on");
                }
                else
                { 
                    string videoId = "";

                    // Parse video ID based on different types of requests
                    if (message.Contains("youtube.com/watch?v="))
                    {
                        int videoIdIndex = message.IndexOf("?v=") + 3;
                        int addParam = message.IndexOf("&", videoIdIndex);

                        if (addParam == -1)
                            videoId = message.Substring(videoIdIndex);
                        else
                            videoId = message.Substring(videoIdIndex, addParam - videoIdIndex);
                    }
                    else if (message.Contains("youtu.be/"))
                    {
                        int videoIdIndex = message.IndexOf("youtu.be/") + 9;
                        int addParam = message.IndexOf("?", videoIdIndex);

                        if (addParam == -1)
                            videoId = message.Substring(videoIdIndex);
                        else
                            videoId = message.Substring(videoIdIndex, addParam - videoIdIndex);
                    }
                    else if (message.Replace("!sr ", "").Length == 11 
                        && message.Replace("!sr ", "").IndexOf(" ") == -1
                        && Regex.Match(message, @"[\w\-]").Success) // assume only video ID
                    {
                        videoId = message.Replace("!sr ", "");
                    }
                    else // search by keyword
                    {
                        string videoKeyword = message.Substring(4);
                        videoId = await _youTubeClientInstance.SearchVideoByKeyword(videoKeyword, 3);
                    }

                    // Confirm if video ID has been found and is a new song request
                    if (string.IsNullOrEmpty(videoId))
                    {
                        _irc.SendPublicChatMessage($"Couldn't find video ID for song request @{username}");
                    }
                    else if (await _youTubeClientInstance.HasDuplicatePlaylistItem(_botConfig.YouTubeBroadcasterPlaylistId, videoId))
                    {
                        _irc.SendPublicChatMessage($"Song has already been requested @{username}");
                    }
                    else
                    {
                        Video video = await _youTubeClientInstance.GetVideoById(videoId, 2);
                        string videoDuration = video.ContentDetails.Duration;

                        // Check if time limit has been reached
                        // ToDo: Make bot setting for duration limit based on minutes (if set)
                        if (!videoDuration.Contains("PT") || videoDuration.Contains("H"))
                        {
                            _irc.SendPublicChatMessage($"Either couldn't find video duration or way too long for the stream @{username}");
                        }
                        else
                        {
                            int timeIndex = videoDuration.IndexOf("T") + 1;
                            string parsedDuration = videoDuration.Substring(timeIndex).TrimEnd('S');
                            int minIndex = parsedDuration.IndexOf("M");

                            string videoMin = "0";
                            string videoSec = "0";
                            int videoMinLimit = 10;
                            int videoSecLimit = 0;

                            if (minIndex > 0)
                                videoMin = parsedDuration.Substring(0, minIndex);

                            videoSec = parsedDuration.Substring(minIndex + 1);

                            if (Convert.ToInt32(videoMin) >= videoMinLimit && Convert.ToInt32(videoSec) >= videoSecLimit)
                            {
                                _irc.SendPublicChatMessage($"Song request is longer than or equal to {videoMinLimit} minute(s) and {videoSecLimit} second(s)");
                            }
                            else
                            {
                                await _youTubeClientInstance.AddVideoToPlaylist(videoId, _botConfig.YouTubeBroadcasterPlaylistId, username);
                                // ToDo: Show playlist position after being able to handle auto-removal of played songs
                                //await Task.Delay(1500); // wait before attempting to get item count from playlist
                                //Playlist broadcasterPlaylist = await _youTubeClientInstance.GetBroadcasterPlaylistById(_botConfig.YouTubeBroadcasterPlaylistId, 1);

                                _irc.SendPublicChatMessage($"@{username} -> \"{video.Snippet.Title}\" by {video.Snippet.ChannelTitle} was successfully requested!"); //+
                                    //$" at position #{broadcasterPlaylist.ContentDetails.ItemCount}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdYouTubeSongRequest(string, string, bool, bool)", false, "!sr");
            }
        }

        public void CmdYouTubeSongRequestList(bool hasYouTubeAuth, bool isYouTubeSongRequestAvail)
        {
            try
            {
                if (hasYouTubeAuth && isYouTubeSongRequestAvail && !string.IsNullOrEmpty(_botConfig.YouTubeBroadcasterPlaylistId))
                {
                    _irc.SendPublicChatMessage($"{_botConfig.Broadcaster.ToLower()}'s song request list is at " +
                        "https://www.youtube.com/playlist?list=" + _botConfig.YouTubeBroadcasterPlaylistId);
                }
                else
                {
                    _irc.SendPublicChatMessage("There is no song request list at this time");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdYouTubeSongRequestList(bool, bool)", false, "!sl");
            }
        }

        public void CmdMultiStreamLink(string username, ref List<string> multiStreamUsers)
        {
            try
            {
                if (multiStreamUsers.Count == 0)
                    _irc.SendPublicChatMessage($"MultiStream link is not set up @{username}");
                else
                {
                    string multiStreamLink = "https://multistre.am/" + _botConfig.Broadcaster.ToLower() + "/";

                    foreach (string multiStreamUser in multiStreamUsers)
                        multiStreamLink += $"{multiStreamUser}/";

                    // Used specifically for multistre.am
                    if (multiStreamUsers.Count == 3)
                        multiStreamLink += "layout11";
                    else if (multiStreamUsers.Count == 2)
                        multiStreamLink += "layout7";
                    else if (multiStreamUsers.Count == 1)
                        multiStreamLink += "layout4";

                    _irc.SendPublicChatMessage($"Check out these awesome streamers at the same time! (Use desktop for best results) {multiStreamLink}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdMultiStream(string, ref List<string>)", false, "!msl");
            }
        }

        public void CmdMagic8Ball(string username)
        {
            try
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                int answerId = rnd.Next(20); // between 0 and 19

                if (answerId == 0)
                    _irc.SendPublicChatMessage($"It is certain @{username}");
                else if (answerId == 1)
                    _irc.SendPublicChatMessage($"It is decidedly so @{username}");
                else if (answerId == 2)
                    _irc.SendPublicChatMessage($"Without a doubt @{username}");
                else if (answerId == 3)
                    _irc.SendPublicChatMessage($"Yes definitely @{username}");
                else if (answerId == 4)
                    _irc.SendPublicChatMessage($"You may rely on it @{username}");
                else if (answerId == 5)
                    _irc.SendPublicChatMessage($"As I see it, yes @{username}");
                else if (answerId == 6)
                    _irc.SendPublicChatMessage($"Most likely @{username}");
                else if (answerId == 7)
                    _irc.SendPublicChatMessage($"Outlook good @{username}");
                else if (answerId == 8)
                    _irc.SendPublicChatMessage($"Yes @{username}");
                else if (answerId == 9)
                    _irc.SendPublicChatMessage($"Signs point to yes @{username}");
                else if (answerId == 10)
                    _irc.SendPublicChatMessage($"Reply hazy try again @{username}");
                else if (answerId == 11)
                    _irc.SendPublicChatMessage($"Ask again later @{username}");
                else if (answerId == 12)
                    _irc.SendPublicChatMessage($"Better not tell you now @{username}");
                else if (answerId == 13)
                    _irc.SendPublicChatMessage($"Cannot predict now @{username}");
                else if (answerId == 14)
                    _irc.SendPublicChatMessage($"Concentrate and ask again @{username}");
                else if (answerId == 15)
                    _irc.SendPublicChatMessage($"Don't count on it @{username}");
                else if (answerId == 16)
                    _irc.SendPublicChatMessage($"My reply is no @{username}");
                else if (answerId == 17)
                    _irc.SendPublicChatMessage($"My sources say no @{username}");
                else if (answerId == 18)
                    _irc.SendPublicChatMessage($"Outlook not so good @{username}");
                else // found largest random value
                    _irc.SendPublicChatMessage($"Very doubtful @{username}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdMagic8Ball(string, string)", false, "!8ball");
            }
        }

        private async Task<bool> ReactionCmd(string origUser, string recipient, string msgToSelf, string action, string addlMsg = "")
        {
            // check if user is trying to use a command on themselves
            if (origUser.Equals(recipient))
            {
                _irc.SendPublicChatMessage(msgToSelf + " @" + origUser);
                return true;
            }

            // check if recipient is the broadcaster before checking the viewer channel
            if (recipient.Equals(_botConfig.Broadcaster.ToLower()) || await ChatterValid(origUser, recipient))
            {
                _irc.SendPublicChatMessage(origUser + " " + action + " @" + recipient + " " + addlMsg);
                return true;
            }

            return false;
        }

        private async Task<bool> ChatterValid(string origUser, string recipient)
        {
            // Check if the requested user is this bot
            if (recipient.Equals(_botConfig.BotName.ToLower()))
                return true;

            // Grab user's chatter info (viewers, mods, etc.)
            List<List<string>> availChatterTypeList = await _twitchInfo.GetChatterListByType();
            if (availChatterTypeList.Count > 0)
            {
                // Search for user
                for (int i = 0; i < availChatterTypeList.Count(); i++)
                {
                    foreach (string chatter in availChatterTypeList[i])
                    {
                        if (chatter.Equals(recipient.ToLower()))
                            return true;
                    }
                }
            }

            // finished searching with no results
            _irc.SendPublicChatMessage("@" + origUser + ": I cannot find the user you wanted to interact with. Perhaps the user left us?");
            return false;
        }

        private string Effectiveness()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int effectiveLvl = rnd.Next(3); // between 0 and 2
            string effectiveness = "";

            if (effectiveLvl == 0)
                effectiveness = "It's super effective!";
            else if (effectiveLvl == 1)
                effectiveness = "It wasn't very effective";
            else
                effectiveness = "It had no effect";

            return effectiveness;
        }
    }
}
