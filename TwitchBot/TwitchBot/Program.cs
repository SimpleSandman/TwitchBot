using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using System.IO;
using RestSharp;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using Tweetinvi;
using Tweetinvi.Core;
using Tweetinvi.Core.Credentials;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using TwitchBot.Configuration;
using System.Collections;
using Autofac;

namespace TwitchBot
{
    class Program
    {
        public static List<Tuple<string, DateTime>> _lstTupDelayMsg = new List<Tuple<string, DateTime>>(); // used to handle delayed msgs

        static void Main(string[] args)
        {
            try
            {
                var appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var twitchBotConfigurationSection = appConfig.GetSection("TwitchBotConfiguration") as TwitchBotConfigurationSection;

                if (twitchBotConfigurationSection == null)
                {
                    //section not in app.config create a default, add it to the config, and save
                    twitchBotConfigurationSection = new TwitchBotConfigurationSection();
                    appConfig.Sections.Add("TwitchBotConfiguration", twitchBotConfigurationSection);
                    appConfig.Save(ConfigurationSaveMode.Full);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    //Since not previously configured, configure bot and save changes using configuration wizard
                    TwitchBotConfigurator.ConfigureBot(twitchBotConfigurationSection);
                    appConfig.Save(ConfigurationSaveMode.Full);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");
                }

                //Bot already configured, do stuff
                //Lets get the connection string, and if it doesn't exist lets run the configuration wizard to add it to the config
                var connectionStringSetting = appConfig.ConnectionStrings.ConnectionStrings["TwitchBotConnStrPROD"];
                if (connectionStringSetting == null)
                {
                    connectionStringSetting = new ConnectionStringSettings();
                    TwitchBotConfigurator.ConfigureConnectionString("TwitchBotConnStrPROD", connectionStringSetting);
                    appConfig.ConnectionStrings.ConnectionStrings.Add(connectionStringSetting);
                    appConfig.Save(ConfigurationSaveMode.Full);
                    ConfigurationManager.RefreshSection("connectionStrings");
                }

                //Create a container builder and register all classes that will be composed for the application
                var builder = new ContainerBuilder();

                builder.RegisterInstance<System.Configuration.Configuration>(appConfig);
                builder.RegisterType<TwitchBotApplication>();

                var container = builder.Build();

                //Define main lifetime scope
                //Get an instance of TwitchBotApplication and execute main loop
                using (var scope = container.BeginLifetimeScope())
                {
                    var app = scope.Resolve<TwitchBotApplication>();
                    Task.WaitAll(app.RunAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Local error found: " + ex.Message);
                Thread.Sleep(3000);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Find the Nth index of a character
        /// </summary>
        /// <param name="s"></param>
        /// <param name="findChar"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int GetNthIndex(string s, char findChar, int n)
        {
            int count = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == findChar)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /*public static void SendTweet(string pendingMessage, string command)
        {
            // Check if there are at least two quotation marks before sending message using LINQ
            string resultMessage = "";
            if (command.Count(c => c == '"') < 2)
            {
                resultMessage = "Please use at least two quotation marks (\") before sending a tweet. " +
                    "Quotations are used to find the start and end of a message wanting to be sent";
                Console.WriteLine(resultMessage);
                _irc.sendPublicChatMessage(resultMessage);
                return;
            }

            // Get message from quotation parameter
            string tweetMessage = string.Empty;
            int length = (pendingMessage.LastIndexOf('"') - pendingMessage.IndexOf('"')) - 1;
            if (length == -1) // if no quotations were found
                length = pendingMessage.Length;
            int startIndex = pendingMessage.IndexOf('"') + 1;
            tweetMessage = pendingMessage.Substring(startIndex, length);

            // Check if message length is at or under 140 characters
            var basicTweet = new object();

            if (tweetMessage.Length <= 140)
            {
                basicTweet = Tweet.PublishTweet(tweetMessage);
                resultMessage = "Tweet successfully published!";
                Console.WriteLine(resultMessage);
                _irc.sendPublicChatMessage(resultMessage);
            }
            else
            {
                int overCharLimit = tweetMessage.Length - 140;
                resultMessage = "The message you attempted to tweet had " + overCharLimit +
                    " characters more than the 140 character limit. Please shorten your message and try again";
                Console.WriteLine(resultMessage);
                _irc.sendPublicChatMessage(resultMessage);
            }
        }

        private static string chatterValid(string strOrigUser, string strRecipient, string strSearchCriteria = "")
        {
            // Check if the requested user is this bot
            if (strRecipient.Equals(_strBotName))
                return "mod";

            // Grab list of chatters (viewers, mods, etc.)
            Chatters chatters = TaskJSON.GetChatters().Result.chatters;

            // check moderators
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("mod"))
            {
                foreach (string moderator in chatters.moderators)
                {
                    if (strRecipient.ToLower().Equals(moderator))
                        return "mod";
                }
            }

            // check viewers
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("viewer"))
            {
                foreach (string viewer in chatters.viewers)
                {
                    if (strRecipient.ToLower().Equals(viewer))
                        return "viewer";
                }
            }

            // check staff
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("staff"))
            {
                foreach (string staffMember in chatters.staff)
                {
                    if (strRecipient.ToLower().Equals(staffMember))
                        return "staff";
                }
            }

            // check admins
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("admin"))
            {
                foreach (string admin in chatters.admins)
                {
                    if (strRecipient.ToLower().Equals(admin))
                        return "admin";
                }
            }

            // check global moderators
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("gmod"))
            {
                foreach (string globalMod in chatters.global_mods)
                {
                    if (strRecipient.ToLower().Equals(globalMod))
                        return "gmod";
                }
            }

            // finished searching with no results
            _irc.sendPublicChatMessage("@" + strOrigUser + ": I cannot find the user you wanted to interact with. Perhaps the user left us?");
            return "";
        }

        public static bool reactionCmd(string message, string strOrigUser, string strRecipient, string strMsgToSelf, string strAction, string strAddlMsg = "")
        {
            string strRoleType = chatterValid(strOrigUser, strRecipient);

            // check if user currently watching the channel
            if (!string.IsNullOrEmpty(strRoleType))
            {
                if (strOrigUser.Equals(strRecipient))
                    _irc.sendPublicChatMessage(strMsgToSelf + " @" + strOrigUser);
                else
                    _irc.sendPublicChatMessage(strOrigUser + " " + strAction + " @" + strRecipient + " " + strAddlMsg);

                return true;
            }
            else
                return false;
        }

        public static int currencyBalance(string username)
        {
            int intBalance = -1;

            // check if user already has a bank account
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBank WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (username.Equals(reader["username"].ToString()))
                                {
                                    intBalance = int.Parse(reader["wallet"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return intBalance;
        }

        public static void updateWallet(string strWalletOwner, int intNewWalletBalance)
        {
            string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intNewWalletBalance;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strWalletOwner;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }



        public static string Effectiveness()
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
        }*/

    }
}
