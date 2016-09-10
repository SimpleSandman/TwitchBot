using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Configuration
{
    public static class TwitchBotConfigurator
    {

        public static void ConfigureBot(TwitchBotConfigurationSection config)
        {
            // do process to configure bots, call into other sections if needed before finishing
            //TODO: There has to be a better way :)
            Console.Clear();
            Console.WriteLine("Configure TwitchBot");
            Console.WriteLine();

            //TODO: add better fail detection, not detecting empty, length, valid characters....
            Console.WriteLine("Bot Name:");
            var botName = Console.ReadLine();

            Console.WriteLine("Broadcaster:");
            var broadcaster = Console.ReadLine();

            Console.WriteLine("Enable Twitter:");
            var enableTwitterString = Console.ReadLine();

            Console.WriteLine("Enable Spotify:");
            var enableSpotifyString = Console.ReadLine();

            Console.WriteLine("Enable Discord:");
            var enableDiscordString = Console.ReadLine();

            config.BotName = botName;
            config.Broadcaster = broadcaster;

            ConfigureTwitch(config);
        }

        public static void ConfigureTwitch(TwitchBotConfigurationSection config)
        {
            Console.WriteLine("Configure TwitchBot Twitch");
            Console.WriteLine();

            //TODO: add better fail detection, not detecting empty, length, valid characters....
            Console.WriteLine("Twitch OAuth Key:");
            var twitchOAuth = Console.ReadLine();

            //TODO: Probably want to hard code this, for release
            Console.WriteLine("Twitch Client Id:");
            var twitchClientId = Console.ReadLine();

            Console.WriteLine("Twitch Access Token:");
            var twitchAccessToken = Console.ReadLine();

            config.TwitchOAuth = twitchOAuth;
            config.TwitchClientId = twitchClientId;
            config.TwitchAccessToken = twitchAccessToken;
        }

        public static void ConfigureTwitter(TwitchBotConfigurationSection config)
        {

        }

        public static void ConfigureConnectionString(string connectionName, ConnectionStringSettings connectionStringSettings)
        {
            //TODO: There has to be a better way :)
            //TODO: Deal with username and password or trusted connection, doing trusted for now
            Console.Clear();
            Console.WriteLine("Configure ConnectionString");
            Console.WriteLine();

            Console.WriteLine("Database Server:");
            var databaseServer = Console.ReadLine();

            Console.WriteLine("Database Name:");
            var databaseName = Console.ReadLine();


            var connStringResult = $"Server={databaseServer.ToString()};Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;";
            connectionStringSettings.Name = connectionName;
            connectionStringSettings.ConnectionString = connStringResult;
        }
    }
}
