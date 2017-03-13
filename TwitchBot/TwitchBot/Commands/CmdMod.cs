using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Extensions;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Commands
{
    public class CmdMod
    {
        private IrcClient _irc;
        private Moderator _modInstance = Moderator.Instance;
        private TimeoutCmd _timeout;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _intBroadcasterID;
        private BankService _bank;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private TwitchInfoService _twitchInfo;

        public CmdMod(IrcClient irc, TimeoutCmd timeout, TwitchBotConfigurationSection botConfig, string connString, int broadcasterId, System.Configuration.Configuration appConfig, BankService bank, TwitchInfoService twitchInfo)
        {
            _irc = irc;
            _timeout = timeout;
            _botConfig = botConfig;
            _intBroadcasterID = broadcasterId;
            _connStr = connString;
            _appConfig = appConfig;
            _bank = bank;
            _twitchInfo = twitchInfo;
        }

        /// <summary>
        /// Displays Discord link (if available)
        /// </summary>
        public void CmdDiscord()
        {
            try
            {
                if (String.IsNullOrEmpty(_botConfig.DiscordLink) || _botConfig.DiscordLink.Equals("Link unavailable at the moment"))
                    _irc.sendPublicChatMessage("Discord link unavailable at the moment");
                else
                    _irc.sendPublicChatMessage("Join me on a wonderful discord server I am proud to be a part of! " + _botConfig.DiscordLink);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdDiscord()", false, "!discord");
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
                    _irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                else
                {
                    int intIndexAction = 8;
                    int intFee = -1;
                    bool validFee = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intFee);
                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    int intWallet = _bank.CheckBalance(strRecipient, _intBroadcasterID);

                    // Check user's bank account exist or has currency
                    if (intWallet == -1)
                        _irc.sendPublicChatMessage("The user '" + strRecipient + "' is not currently banking with us @" + strUserName);
                    else if (intWallet == 0)
                        _irc.sendPublicChatMessage("'" + strRecipient + "' is out of " + _botConfig.CurrencyType + " @" + strUserName);
                    // Check if fee can be accepted
                    else if (intFee > 0)
                        _irc.sendPublicChatMessage("Please insert a negative whole amount (no decimal numbers) "
                            + " or use the !deposit command to add " + _botConfig.CurrencyType + " to a user's account");
                    else if (!validFee)
                        _irc.sendPublicChatMessage("The fee wasn't accepted. Please try again with negative whole amount (no decimals)");
                    else /* Deduct funds from wallet */
                    {
                        intWallet += intFee;

                        // Zero out account balance if user is being charged more than they have
                        if (intWallet < 0)
                            intWallet = 0;

                        _bank.UpdateFunds(strRecipient, _intBroadcasterID, intWallet);

                        // Prompt user's balance
                        if (intWallet == 0)
                            _irc.sendPublicChatMessage("Charged " + intFee.ToString().Replace("-", "") + " " + _botConfig.CurrencyType + " to " + strRecipient
                                + "'s account! They are out of " + _botConfig.CurrencyType + " to spend");
                        else
                            _irc.sendPublicChatMessage("Charged " + intFee.ToString().Replace("-", "") + " " + _botConfig.CurrencyType + " to " + strRecipient
                                + "'s account! They only have " + intWallet + " " + _botConfig.CurrencyType + " to spend");
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdCharge(string, string)", false, "!charge");
            }
        }

        /// <summary>
        /// Gives a set amount of stream currency to user
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdDeposit(string message, string strUserName)
        {
            try
            {
                List<string> userList = new List<string>();

                foreach (int index in message.AllIndexesOf("@"))
                {
                    int lengthUsername = message.IndexOf(" ", index) - index - 1;
                    if (lengthUsername < 0)
                        userList.Add(message.Substring(index + 1).ToLower());
                    else
                        userList.Add(message.Substring(index + 1, lengthUsername).ToLower());
                }

                // Check for valid command
                if (message.StartsWith("!deposit @"))
                    _irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                // Check if moderator is trying to give money to themselves
                else if (_modInstance.LstMod.Contains(strUserName.ToLower()) && userList.Contains(strUserName.ToLower()))
                    _irc.sendPublicChatMessage($"Entire deposit voided. You cannot add funds to your own account @{strUserName}");
                else
                {
                    int intIndexAction = 9;
                    int intDeposit = -1;
                    bool validDeposit = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intDeposit);

                    // Check if deposit amount is valid
                    if (intDeposit < 0)
                        _irc.sendPublicChatMessage("Please insert a positive whole amount (no decimals) " 
                            + " or use the !charge command to remove " + _botConfig.CurrencyType + " from a user");
                    else if (!validDeposit)
                        _irc.sendPublicChatMessage("The deposit wasn't accepted. Please try again with positive whole amount (no decimals)");
                    else
                    {
                        if (userList.Count > 0)
                        {
                            List<BalanceResult> lstBalRes = _bank.UpdateCreateBalance(userList, _intBroadcasterID, intDeposit, true);

                            string responseMsg = $"Gave {intDeposit.ToString()} {_botConfig.CurrencyType} to ";

                            if (lstBalRes.Count > 1)
                            {
                                foreach (BalanceResult userResult in lstBalRes)
                                    responseMsg += $"@{userResult.Username} ";
                            }
                            else if (lstBalRes.Count == 1)
                            {
                                responseMsg += $"@{lstBalRes[0].Username} ";

                                if (lstBalRes[0].ActionType.Equals("UPDATE"))
                                    responseMsg += $"and now has {lstBalRes[0].Wallet} {_botConfig.CurrencyType}!";
                                else if (lstBalRes[0].ActionType.Equals("INSERT"))
                                    responseMsg += $"and can now gamble it all away! Kappa";
                            }
                            else
                                responseMsg = $"Unknown error has occurred in retrieving results. Please check your recipient's {_botConfig.CurrencyType}";

                            _irc.sendPublicChatMessage(responseMsg);
                        }
                        else
                        {
                            _irc.sendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{strUserName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdDeposit(string, string)", false, "!deposit");
            }
        }

        /// <summary>
        /// Gives every viewer currently watching a set amount of currency
        /// </summary>
        /// <param name="message"></param>
        /// <param name="strUserName"></param>
        public async Task CmdBonusAll(string message, string strUserName)
        {
            try
            {
                // Check for valid command
                if (message.StartsWith("!bonusall @"))
                    _irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                else
                {
                    int intIndexAction = 10;
                    int intDeposit = -1;
                    bool validDeposit = int.TryParse(message.Substring(intIndexAction), out intDeposit);

                    // Check if deposit amount is valid
                    if (intDeposit < 0)
                        _irc.sendPublicChatMessage("Please insert a positive whole amount (no decimals) "
                            + " or use the !charge command to remove " + _botConfig.CurrencyType + " from a user");
                    else if (!validDeposit)
                        _irc.sendPublicChatMessage("The bulk deposit wasn't accepted. Please try again with positive whole amount (no decimals)");
                    else
                    {
                        List<string> chatterList = await _twitchInfo.GetChatterList();
                        chatterList = chatterList.Where(t => t != _botConfig.Broadcaster.ToLower() && t != _botConfig.BotName.ToLower()).ToList();

                        if (chatterList != null && chatterList.Count > 0)
                        {
                            _bank.UpdateCreateBalance(chatterList, _intBroadcasterID, intDeposit);
                            _irc.sendPublicChatMessage($"{intDeposit.ToString()} {_botConfig.CurrencyType} for everyone! "
                                + $"Check your stream bank account with !{_botConfig.CurrencyType.ToLower()}");
                        }
                        else
                        {
                            _irc.sendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{strUserName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdBonusAll(string, string)", false, "!bonusall");
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
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                    {
                        cmd.Parameters.AddWithValue("@broadcaster", _intBroadcasterID);
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
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    _irc.sendPublicChatMessage("The first song in queue, '" + strRemovedSong + "' has been removed from the request list");
                }
                else
                    _irc.sendPublicChatMessage("There are no songs that can be removed from the song request list");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdPopSongRequest()", false, "!popsr");
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
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) partyMember, username FROM tblPartyUpRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                    {
                        cmd.Parameters.AddWithValue("@broadcaster", _intBroadcasterID);
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
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    _irc.sendPublicChatMessage("The first party member in queue, '" + strRemovedPartyMember + "' has been removed from the request list");
                }
                else
                    _irc.sendPublicChatMessage("There are no songs that can be removed from the song request list");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdPopPartyUpRequest()", false, "!poppartyuprequest");
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
                    _irc.sendPublicChatMessage("I cannot make a user not talk to me without this format '!addtimeout [seconds] @[username]'");
                else if (message.ToLower().Contains(_botConfig.Broadcaster.ToLower()))
                    _irc.sendPublicChatMessage("I cannot betray @" + _botConfig.Broadcaster + " by not allowing him to communicate with me @" + strUserName);
                else if (message.ToLower().Contains(_botConfig.BotName.ToLower()))
                    _irc.sendPublicChatMessage("You can't time me out @" + strUserName);
                else
                {
                    int intIndexAction = 12;
                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    double dblSec = -1;
                    bool validDeposit = double.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out dblSec);

                    if (!validDeposit || dblSec < 0.00)
                        _irc.sendPublicChatMessage("The timeout amount wasn't accepted. Please try again with positive seconds only");
                    else if (dblSec < 15.00)
                        _irc.sendPublicChatMessage("The duration needs to be at least 15 seconds long. Please try again");
                    else
                    {
                        _timeout.addTimeoutToLst(strRecipient, _intBroadcasterID, dblSec, _connStr);

                        _irc.sendPublicChatMessage("I am told not to talk to you for " + dblSec + " seconds @" + strRecipient);
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdAddTimeout(string, string)", false, "!addtimeout");
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

                _timeout.delTimeoutFromLst(strRecipient, _intBroadcasterID, _connStr);

                _irc.sendPublicChatMessage(strRecipient + " can now interact with me again because of @" + strUserName);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdDelTimeout(string, string)", false, "!deltimeout");
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
                    _irc.sendPublicChatMessage("Please insert a valid positive alloted amount of time (in seconds)");
                else
                {
                    // set and save latency
                    _botConfig.StreamLatency = intLatency;
                    _appConfig.Save();

                    Console.WriteLine("Stream latency set to " + _botConfig.StreamLatency + " second(s)");
                    _irc.sendPublicChatMessage("Bot settings for stream latency set to " + _botConfig.StreamLatency + " second(s) @" + strUserName);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdSetLatency(string, string)", false, "!setlatency");
            }
        }

        /// <summary>
        /// Add a mod/broadcaster quote
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdAddQuote(string message, string strUserName)
        {
            try
            {
                string strQuote = message.Substring(message.IndexOf(" ") + 1);

                string query = "INSERT INTO tblQuote (userQuote, username, timeCreated, broadcaster) VALUES (@userQuote, @username, @timeCreated, @broadcaster)";

                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@userQuote", SqlDbType.VarChar, 500).Value = strQuote;
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strUserName;
                    cmd.Parameters.Add("@timeCreated", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                _irc.sendPublicChatMessage($"Quote has been created @{strUserName}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdAddQuote(string, string)", false, "!addquote");
            }
        }

        /// <summary>
        /// Tell the stream the specified moderator will be AFK
        /// </summary>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdModAfk(string strUserName)
        {
            try
            {
                _irc.sendPublicChatMessage($"@{strUserName} is going AFK!");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdModAfk(string)", false, "!modafk");
            }
        }

        /// <summary>
        /// Tell the stream the specified moderator is back
        /// </summary>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdModBack(string strUserName)
        {
            try
            {
                _irc.sendPublicChatMessage($"@{strUserName} is back!");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdMod", "CmdModBack(string)", false, "!modback");
            }
        }

    }
}
