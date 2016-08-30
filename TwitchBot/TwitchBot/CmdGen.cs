using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace TwitchBot
{
    public class CmdGen
    {
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
                if (Program.GetStream().Result.stream != null)
                {
                    string strDuration = Program.GetStream().Result.stream.created_at;
                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(strDuration, new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal);
                    string strResultDuration = String.Format("{0:h\\:mm\\:ss}", ts);
                    Program._irc.sendPublicChatMessage("This channel's current uptime (length of current stream) is " + strResultDuration);
                }
                else
                    Program._irc.sendPublicChatMessage("This channel is not live anything at the moment");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdGen", "CmdDuration(string)", false, "!duration");
            }
        }

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

        public void CmdSR(bool isSongRequestAvail, string strMessage, string strUserName)
        {
            try
            {
                // Check if song request system is enabled
                if (isSongRequestAvail)
                {
                    // Grab the song name from the request
                    int index = strMessage.IndexOf("!sr");
                    string songRequest = strMessage.Substring(index, strMessage.Length - index);
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
    }
}
