using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using System.Net.Http;

using Autofac;

using TwitchBot.Configuration;
using TwitchBot.Modules;
using TwitchBot.Models;

namespace TwitchBot
{
    class Program
    {
        public static string ConnStrType = "TwitchBotConnStrPROD"; // assume prod connection string by default
        public static List<DelayedMessage> DelayedMessages = new List<DelayedMessage>(); // used to handle delayed msgs
        public static List<RouletteUser> RouletteUsers = new List<RouletteUser>(); // used to handle russian roulette
        public static readonly HttpClient HttpClient = new HttpClient();

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
                        ConnStrType = "TwitchBotConnStrTEST";
                    }
                }

                //Create a container builder and register all classes that will be composed for the application
                var builder = new ContainerBuilder();

                builder.RegisterModule(new TwitchBotModule()
                {
                    AppConfig = appConfig,
                    ConnectionString = new NamedParameter("connStr", connectionStringSetting.ConnectionString),
                    TwitchBotApiLink = new NamedParameter("twitchBotApiLink", twitchBotConfigurationSection.TwitchBotApiLink),
                    TwitchBotConfigurationSection = twitchBotConfigurationSection
                });

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
    }
}
