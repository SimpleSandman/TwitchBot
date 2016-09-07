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

namespace TwitchBot
{
    public class CmdGen
    {
        public void CmdCmds()
        {
            try
            {
                Program._irc.sendPublicChatMessage("--- !hello | !slap @[username] | !stab @[username] | !throw [item] @[username] | !shoot @[username]"
                    + "| !spotifycurr | !srlist | !sr [artist] - [song title] | !utctime | !hosttime | !partyup [party member name] ---"
                    + " Link to full list of commands: "
                    + "https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdCmds()", false, "!cmds");
            }
        }

        public void CmdHello(string strUserName)
        {
            try
            {
                Program._irc.sendPublicChatMessage($"Hey @{strUserName}! Thanks for talking to me.");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdHello(string)", false, "!hello");
            }
        }

        public void CmdUtcTime()
        {
            try
            {
                Program._irc.sendPublicChatMessage($"UTC Time: {DateTime.UtcNow.ToString()}");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdUtcTime()", false, "!utctime");
            }
        }

        public void CmdHostTime(string strBroadcasterName)
        {
            try
            {
                Program._irc.sendPublicChatMessage($"{strBroadcasterName}'s Current Time: {DateTime.Now.ToString()} ({TimeZone.CurrentTimeZone.StandardName})");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdHostTime(string)", false, "!hosttime");
            }
        }

        public void CmdDuration()
        {
            try
            {
                // Check if the channel is live
                if (TaskJSON.GetStream().Result.stream != null)
                {
                    string strDuration = TaskJSON.GetStream().Result.stream.created_at;
                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(strDuration, new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal);
                    string strResultDuration = String.Format("{0:h\\:mm\\:ss}", ts);
                    Program._irc.sendPublicChatMessage("This channel's current uptime (length of current stream) is " + strResultDuration);
                }
                else
                    Program._irc.sendPublicChatMessage("This channel is not streaming anything at the moment");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdDuration(string)", false, "!duration");
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

                using (SqlConnection conn = new SqlConnection(Program._connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
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
                                        songList = songList + newRow[col.ColumnName].ToString() + " || ";
                                    }
                                }
                                StringBuilder strBdrSongList = new StringBuilder(songList);
                                strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " || "
                                songList = strBdrSongList.ToString(); // replace old song list string with new
                                Program._irc.sendPublicChatMessage("Current List of Requested Songs: " + songList);
                            }
                            else
                            {
                                Console.WriteLine("No requests have been made");
                                Program._irc.sendPublicChatMessage("No requests have been made");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdSRList()", false, "!srlist");
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
                        Program._irc.sendPublicChatMessage("Only letters, numbers, hyphens (-), parentheses (), " 
                            + "apostrophes ('), and question marks (?) are allowed. Please try again. "
                            + "If the problem persists, please contact my creator");
                    }
                    else
                    {
                        /* Add song request to database */
                        string query = "INSERT INTO tblSongRequests (songRequests, broadcaster, chatter) VALUES (@song, @broadcaster, @chatter)";

                        using (SqlConnection conn = new SqlConnection(Program._connStr))
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.Add("@song", SqlDbType.VarChar, 200).Value = songRequest;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.VarChar, 200).Value = Program._intBroadcasterID;
                            cmd.Parameters.Add("@chatter", SqlDbType.VarChar, 200).Value = strUserName;

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }

                        Program._irc.sendPublicChatMessage("The song \"" + songRequest + "\" has been successfully requested!");
                    }
                }
                else
                    Program._irc.sendPublicChatMessage("Song requests are disabled at the moment");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdSR(bool, string, string)", false, "!sr");
            }
        }

        /// <summary>
        /// Displays the current song being played from Spotify
        /// </summary>
        public void CmdSpotifyCurr()
        {
            try
            {
                StatusResponse status = Program._spotify.GetStatus();
                if (status != null)
                {
                    Program._irc.sendPublicChatMessage("Current Song: " + status.Track.TrackResource.Name
                        + " || Artist: " + status.Track.ArtistResource.Name
                        + " || Album: " + status.Track.AlbumResource.Name);
                }
                else
                    Program._irc.sendPublicChatMessage("The broadcaster is not playing a song at the moment");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdSpotifyCurr()", false, "!spotifycurr");
            }
        }

        /// <summary>
        /// Slaps a user and rates its effectiveness
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdSlap(string message, string strUserName)
        {
            try
            {
                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                Program.reactionCmd(message, strUserName, strRecipient, "Stop smacking yourself", "slaps", Program.Effectiveness());
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdSlap(string, string)", false, "!slap");
            }
        }

        /// <summary>
        /// Stabs a user and rates its effectiveness
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdStab(string message, string strUserName)
        {
            try
            {
                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                Program.reactionCmd(message, strUserName, strRecipient, "Stop stabbing yourself", "stabs", Program.Effectiveness());
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdStab(string, string)", false, "!stab");
            }
        }

        /// <summary>
        /// Shoots a viewer's random body part
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdShoot(string message, string strUserName)
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

                Program.reactionCmd(message, strUserName, strRecipient, "You just shot your " + strBodyPart.Replace("'s ", ""), "shoots", strBodyPart);

                // bot responds if targeted
                if (strRecipient.Equals(Program._strBotName.ToLower()))
                {
                    if (strBodyPart.Equals(" but missed"))
                        Program._irc.sendPublicChatMessage("Ha! You missed @" + strUserName);
                    else
                        Program._irc.sendPublicChatMessage("You think shooting me in the " + strBodyPart.Replace("'s ", "") + " would hurt me? I am a bot!");
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdShoot(string, string)", false, "!shoot");
            }
        }

        /// <summary>
        /// Throws an item at a viewer and rates its effectiveness against the victim
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdThrow(string message, string strUserName)
        {
            try
            {
                int intIndexAction = 7;

                if (message.StartsWith("!throw @"))
                    Program._irc.sendPublicChatMessage("Please throw an item to a user @" + strUserName);
                else
                {
                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    string item = message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1);

                    Program.reactionCmd(message, strUserName, strRecipient, "Stop throwing " + item + " at yourself", "throws " + item + " at", ". " + Program.Effectiveness());
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdThrow(string, string)", false, "!throw");
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
                Program._strBroadcasterGame = TaskJSON.GetChannel().Result.game;

                // check if user entered something
                if (message.Length < intInputIndex)
                    Program._irc.sendPublicChatMessage("Please enter a party member @" + strUserName);
                else
                    strPartyMember = message.Substring(intInputIndex);

                // grab game id in order to find party member
                using (SqlConnection conn = new SqlConnection(Program._connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Program._strBroadcasterGame.Equals(reader["name"].ToString()))
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
                    Program._irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else // check if user has already requested a party member
                {
                    using (SqlConnection conn = new SqlConnection(Program._connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUpRequests "
                            + "WHERE broadcaster = @broadcaster AND game = @game AND username = @username", conn))
                        {
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
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
                        Program._irc.sendPublicChatMessage("You have already requested a party member. Please wait until your request has been completed @" + strUserName);
                    else // search for party member user is requesting
                    {
                        using (SqlConnection conn = new SqlConnection(Program._connStr))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUp WHERE broadcaster = @broadcaster AND game = @game", conn))
                            {
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
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
                            Program._irc.sendPublicChatMessage("I couldn't find the requested party member '" + strPartyMember + "' @" + strUserName
                                + ". Please check with the broadcaster for possible spelling errors");
                        else
                        {
                            string query = "INSERT INTO tblPartyUpRequests (username, partyMember, timeRequested, broadcaster, game) "
                                + "VALUES (@username, @partyMember, @timeRequested, @broadcaster, @game)";

                            // Create connection and command
                            using (SqlConnection conn = new SqlConnection(Program._connStr))
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = strUserName;
                                cmd.Parameters.Add("@partyMember", SqlDbType.VarChar, 50).Value = strPartyMember;
                                cmd.Parameters.Add("@timeRequested", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                                conn.Close();
                            }

                            Program._irc.sendPublicChatMessage("@" + strUserName + ": " + strPartyMember + " has been added to the party queue");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdPartyUp(string, string)", false, "!partyup");
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
                Program._strBroadcasterGame = TaskJSON.GetChannel().Result.game;

                // grab game id in order to find party member
                using (SqlConnection conn = new SqlConnection(Program._connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Program._strBroadcasterGame.Equals(reader["name"].ToString()))
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
                    Program._irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else
                {
                    using (SqlConnection conn = new SqlConnection(Program._connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT username, partyMember FROM tblPartyUpRequests "
                            + "WHERE game = @game AND broadcaster = @broadcaster ORDER BY Id", conn))
                        {
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        strPartyList += reader["partyMember"].ToString() + " <-- " + reader["username"].ToString() + " || ";
                                    }
                                    StringBuilder strBdrPartyList = new StringBuilder(strPartyList);
                                    strBdrPartyList.Remove(strPartyList.Length - 4, 4); // remove extra " || "
                                    strPartyList = strBdrPartyList.ToString(); // replace old party member list string with new
                                    Program._irc.sendPublicChatMessage(strPartyList);
                                }
                                else
                                {
                                    Console.WriteLine("No party members are set for this game");
                                    Program._irc.sendPublicChatMessage("No party members are set for this game");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdPartyUpRequestList()", false, "!partyuprequestlist");
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
                Program._strBroadcasterGame = TaskJSON.GetChannel().Result.game;

                // grab game id in order to find party member
                using (SqlConnection conn = new SqlConnection(Program._connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Program._strBroadcasterGame.Equals(reader["name"].ToString()))
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
                    Program._irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                else
                {
                    using (SqlConnection conn = new SqlConnection(Program._connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT partyMember FROM tblPartyUp WHERE game = @game AND broadcaster = @broadcaster ORDER BY partyMember", conn))
                        {
                            cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        strPartyList += reader["partyMember"].ToString() + " || ";
                                    }
                                    StringBuilder strBdrPartyList = new StringBuilder(strPartyList);
                                    strBdrPartyList.Remove(strPartyList.Length - 4, 4); // remove extra " || "
                                    strPartyList = strBdrPartyList.ToString(); // replace old party member list string with new
                                    Program._irc.sendPublicChatMessage(strPartyList);
                                }
                                else
                                {
                                    Console.WriteLine("No party members are set for this game");
                                    Program._irc.sendPublicChatMessage("No party members are set for this game");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdPartyUpList()", false, "!partyuplist");
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
                int intBalance = Program.currencyBalance(strUserName);

                if (intBalance == -1)
                    Program._irc.sendPublicChatMessage("You are not currently banking with us at the moment. Please talk to a moderator about acquiring " + Program._strCurrencyType);
                else
                    Program._irc.sendPublicChatMessage("@" + strUserName + " currently has " + intBalance.ToString() + " " + Program._strCurrencyType);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdCheckFunds()", false, "!myfunds");
            }
        }
    }
}
