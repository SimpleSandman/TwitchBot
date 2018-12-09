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
        public static List<DelayedMessage> DelayedMessages = new List<DelayedMessage>(); // used to handle delayed msgs
        public static List<RouletteUser> RouletteUsers = new List<RouletteUser>(); // used to handle russian roulette
        public static readonly HttpClient HttpClient = new HttpClient();
        public static List<string> TwitchUrls = new List<string> { "https://clips.twitch.tv/" }; // ToDo: Please put this somewhere else!!!

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

                //Create a container builder and register all classes that will be composed for the application
                var builder = new ContainerBuilder();

                builder.RegisterModule(new TwitchBotModule()
                {
                    AppConfig = appConfig,
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
