using Autofac;

using TwitchBot.Configuration;
using TwitchBot.Repositories;
using TwitchBot.Services;
using TwitchBot.Threads;

namespace TwitchBot.Modules
{
    public class TwitchBotModule : Module
    {
        public System.Configuration.Configuration AppConfig { get; set; }
        public Autofac.Core.Parameter TwitchBotApiLink { get; set; }
        public TwitchBotConfigurationSection TwitchBotConfigurationSection { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // configuration
            builder.RegisterInstance(AppConfig);
            builder.RegisterInstance(TwitchBotConfigurationSection);

            // main app
            builder.RegisterType<TwitchBotApplication>();

            // repositories
            builder.RegisterType<BankRepository>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<FollowerRepository>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<SongRequestBlacklistRepository>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<ManualSongRequestRepository>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<PartyUpRepository>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<GameDirectoryRepository>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<QuoteRepository>()
                .WithParameter(TwitchBotApiLink);

            // services
            builder.RegisterType<BankService>();
            builder.RegisterType<FollowerService>();
            builder.RegisterType<SongRequestBlacklistService>();
            builder.RegisterType<ManualSongRequestService>();
            builder.RegisterType<PartyUpService>();
            builder.RegisterType<GameDirectoryService>();
            builder.RegisterType<QuoteService>();
            builder.RegisterType<TwitchInfoService>();

            // threads
            builder.RegisterType<FollowerSubscriberListener>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<TwitchChatterListener>();
            builder.RegisterType<BankHeist>()
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<BossFight>()
                .WithParameter(TwitchBotApiLink);
        }
    }
}
