using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class CmdMod
    {
        /// <summary>
        /// Displays Discord link (if available)
        /// </summary>
        public void CmdDiscord()
        {
            try
            {
                if (String.IsNullOrEmpty(Program._strDiscordLink) || Program._strDiscordLink.Equals("Link unavailable at the moment"))
                    Program._irc.sendPublicChatMessage("Discord link unavailable at the moment");
                else
                    Program._irc.sendPublicChatMessage("Join me on a wonderful discord server I am proud to be a part of! " + Program._strDiscordLink);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdDiscord()", false, "!discord");
            }
        }

        /// <summary>
        /// Takes money away from a user
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdCharge(string message, string strUserName)
        {
            try
            {
                if (message.StartsWith("!charge @"))
                    Program._irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                else
                {
                    int intIndexAction = 8;
                    int intFee = -1;
                    bool validFee = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intFee);
                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    int intWallet = Program.currencyBalance(strRecipient);

                    // Check user's bank account
                    if (intWallet == -1)
                        Program._irc.sendPublicChatMessage("The user '" + strRecipient + "' is not currently banking with us @" + strUserName);
                    else if (intWallet == 0)
                        Program._irc.sendPublicChatMessage("'" + strRecipient + "' is out of " + Program._strCurrencyType + " @" + strUserName);
                    // Check if fee can be accepted
                    else if (intFee > 0)
                        Program._irc.sendPublicChatMessage("Please insert a negative amount or use the !deposit command to add " + Program._strCurrencyType + " to a user");
                    else if (!validFee)
                        Program._irc.sendPublicChatMessage("The fee wasn't accepted. Please try again with negative whole numbers only");
                    else /* Insert fee from wallet */
                    {
                        intWallet = intWallet + intFee;

                        // Zero out account balance if user is being charged more than they have
                        if (intWallet < 0)
                            intWallet = 0;

                        string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

                        using (SqlConnection conn = new SqlConnection(Program._connStr))
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intWallet;
                            cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                        // Prompt user's balance
                        if (intWallet == 0)
                            Program._irc.sendPublicChatMessage("Charged " + intFee.ToString().Replace("-", "") + " " + Program._strCurrencyType + " to " + strRecipient
                                + "'s account! They are out of " + Program._strCurrencyType + " to spend");
                        else
                            Program._irc.sendPublicChatMessage("Charged " + intFee.ToString().Replace("-", "") + " " + Program._strCurrencyType + " to " + strRecipient
                                + "'s account! They only have " + intWallet + " " + Program._strCurrencyType + " to spend");
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdCharge(string, string)", false, "!charge");
            }
        }

        /// <summary>
        /// Gives money to user
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdDeposit(string message, string strUserName)
        {
            try
            {
                // Check for valid command
                if (message.StartsWith("!deposit @"))
                    Program._irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                // Check if moderator is trying to give money to themselves
                else if (Program._mod.getLstMod().Contains(strUserName.ToLower()))
                    Program._irc.sendPublicChatMessage("You cannot add funds to your own account @" + strUserName);
                else
                {
                    int intIndexAction = 9;
                    int intDeposit = -1;
                    bool validDeposit = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intDeposit);
                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    int intWallet = Program.currencyBalance(strRecipient);

                    // Check if deposit amount is valid
                    if (intDeposit < 0)
                        Program._irc.sendPublicChatMessage("Please insert a positive amount or use the !charge command to remove " + Program._strCurrencyType + " from a user");
                    else if (!validDeposit)
                        Program._irc.sendPublicChatMessage("The deposit wasn't accepted. Please try again with positive whole numbers only");
                    else
                    {
                        // Check if user has a bank account
                        if (intWallet == -1)
                        {
                            string query = "INSERT INTO tblBank (username, wallet, broadcaster) VALUES (@username, @wallet, @broadcaster)";

                            using (SqlConnection conn = new SqlConnection(Program._connStr))
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intDeposit;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }

                            Program._irc.sendPublicChatMessage(strUserName + " has created a new account for @" + strRecipient
                                + " with " + intDeposit + " " + Program._strCurrencyType + " to spend");
                        }
                        else // Deposit money into wallet
                        {
                            intWallet = intWallet + intDeposit;

                            string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

                            using (SqlConnection conn = new SqlConnection(Program._connStr))
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intWallet;
                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }

                            // Prompt user's balance
                            Program._irc.sendPublicChatMessage("Deposited " + intDeposit.ToString() + " " + Program._strCurrencyType + " to @" + strRecipient
                                + "'s account! They now have " + intWallet + " " + Program._strCurrencyType + " to spend");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdDeposit(string, string)", false, "!deposit");
            }
        }

        /// <summary>
        /// Removes the first song in the queue of song requests
        /// </summary>
        public void CmdPopSongRequest()
        {
            string strRemovedSong = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(Program._connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                    {
                        cmd.Parameters.AddWithValue("@broadcaster", Program._intBroadcasterID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    strRemovedSong = reader["songRequests"].ToString();
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(strRemovedSong))
                {
                    string query = "WITH T AS (SELECT TOP(1) * FROM tblSongRequests WHERE broadcaster = @broadcaster ORDER BY id) DELETE FROM T";

                    // Create connection and command
                    using (SqlConnection conn = new SqlConnection(Program._connStr))
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    Program._irc.sendPublicChatMessage("The first song in queue, '" + strRemovedSong + "' has been removed from the request list");
                }
                else
                    Program._irc.sendPublicChatMessage("There are no songs that can be removed from the song request list");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdPopSongRequest()", false, "!popsr");
            }
        }

        /// <summary>
        /// Removes first party memeber in queue of party up requests
        /// </summary>
        public void CmdPopPartyUpRequest()
        {
            string strRemovedPartyMember = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(Program._connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) partyMember, username FROM tblPartyUpRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                    {
                        cmd.Parameters.AddWithValue("@broadcaster", Program._intBroadcasterID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    strRemovedPartyMember = reader["partyMember"].ToString();
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(strRemovedPartyMember))
                {
                    string query = "WITH T AS (SELECT TOP(1) * FROM tblPartyUpRequests WHERE broadcaster = @broadcaster ORDER BY id) DELETE FROM T";

                    // Create connection and command
                    using (SqlConnection conn = new SqlConnection(Program._connStr))
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    Program._irc.sendPublicChatMessage("The first party member in queue, '" + strRemovedPartyMember + "' has been removed from the request list");
                }
                else
                    Program._irc.sendPublicChatMessage("There are no songs that can be removed from the song request list");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdPopPartyUpRequest()", false, "!poppartyuprequest");
            }
        }

        /// <summary>
        /// Bot-specific timeout on a user for a set amount of time
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdAddTimeout(string message, string strUserName)
        {
            try
            {
                if (message.StartsWith("!addtimeout @"))
                    Program._irc.sendPublicChatMessage("I cannot make a user not talk to me without this format '!addtimeout [seconds] @[username]'");
                else if (message.ToLower().Contains(Program._strBroadcasterName.ToLower()))
                    Program._irc.sendPublicChatMessage("I cannot betray @" + Program._strBroadcasterName + " by not allowing him to communicate with me @" + strUserName);
                else if (message.ToLower().Contains(Program._strBotName.ToLower()))
                    Program._irc.sendPublicChatMessage("You can't time me out from my own commands @" + strUserName);
                else
                {
                    int intIndexAction = 12;
                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    double dblSec = -1;
                    bool validDeposit = double.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out dblSec);

                    if (!validDeposit || dblSec < 0.00)
                        Program._irc.sendPublicChatMessage("The timeout amount wasn't accepted. Please try again with positive seconds only");
                    else if (dblSec < 15.00)
                        Program._irc.sendPublicChatMessage("The duration needs to be at least 15 seconds long. Please try again");
                    else
                    {
                        Program._timeout.addTimeoutToLst(strRecipient, Program._intBroadcasterID, dblSec, Program._connStr);

                        Program._irc.sendPublicChatMessage("I am told not to talk to you for " + dblSec + " seconds @" + strRecipient);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdAddTimeout(string, string)", false, "!addtimeout");
            }
        }

        /// <summary>
        /// Remove bot-specific timeout on a user for a set amount of time
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdDelTimeout(string message, string strUserName)
        {
            try
            {
                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();

                Program._timeout.delTimeoutFromLst(strRecipient, Program._intBroadcasterID, Program._connStr);

                Program._irc.sendPublicChatMessage(strRecipient + " can now interact with me again because of @" + strUserName);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdDelTimeout(string, string)", false, "!deltimeout");
            }
        }

        /// <summary>
        /// Set delay for messages based on the latency of the stream
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdSetLatency(string message, string strUserName)
        {
            try
            {
                int intLatency = -1;
                bool validInput = int.TryParse(message.Substring(12), out intLatency);
                if (!validInput || intLatency < 0)
                    Program._irc.sendPublicChatMessage("Please insert a valid positive alloted amount of time (in seconds)");
                else
                {
                    // set and save latency
                    Program._intStreamLatency = intLatency;
                    Properties.Settings.Default.streamLatency = Program._intStreamLatency;
                    Properties.Settings.Default.Save();

                    Console.WriteLine("Stream latency set to " + Program._intStreamLatency + " second(s)");
                    Program._irc.sendPublicChatMessage("Bot settings for stream latency set to " + Program._intStreamLatency + " second(s) @" + strUserName);
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdSetLatency(string, string)", false, "!setlatency");
            }
        }
    }
}
