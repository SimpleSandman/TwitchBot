using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using Autofac;

using TwitchBot.Configuration;
using TwitchBot.Repositories;
using TwitchBot.Services;

namespace TwitchBot
{
    class Program
    {
        public static string _connStrType = "TwitchBotConnStrPROD"; // assume prod connection string by default
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
                    // check if testing connection string is valid (for localized development only)
                    connectionStringSetting = appConfig.ConnectionStrings.ConnectionStrings["TwitchBotConnStrTEST"];
                    if (connectionStringSetting == null)
                    {
                        // go ahead and run the production configuration wizard
                        connectionStringSetting = new ConnectionStringSettings();
                        TwitchBotConfigurator.ConfigureConnectionString("TwitchBotConnStrPROD", connectionStringSetting);
                        appConfig.ConnectionStrings.ConnectionStrings.Add(connectionStringSetting);
                        appConfig.Save(ConfigurationSaveMode.Full);
                        ConfigurationManager.RefreshSection("connectionStrings");
                    }
                    else
                    {
                        // found test connection string
                        _connStrType = "TwitchBotConnStrTEST";
                    }
                }

                //Create a container builder and register all classes that will be composed for the application
                var builder = new ContainerBuilder();

                builder.RegisterInstance(appConfig);
                builder.RegisterInstance(twitchBotConfigurationSection);

                builder.RegisterType<TwitchBotApplication>();

                // repositories
                builder.RegisterType<FollowerRepository>().WithParameter(new TypedParameter(typeof(string), connectionStringSetting.ConnectionString));

                // services
                builder.RegisterType<FollowerService>();
                builder.RegisterType<TwitchInfoService>();               

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
    }
}
