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
        public Autofac.Core.Parameter ConnectionString { get; set; }
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
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<FollowerRepository>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<SongRequestBlacklistRepository>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<ManualSongRequestRepository>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<PartyUpRepository>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<GameDirectoryRepository>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<QuoteRepository>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<GiveawayRepository>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);

            // services
            builder.RegisterType<BankService>();
            builder.RegisterType<FollowerService>();
            builder.RegisterType<SongRequestBlacklistService>();
            builder.RegisterType<ManualSongRequestService>();
            builder.RegisterType<PartyUpService>();
            builder.RegisterType<GameDirectoryService>();
            builder.RegisterType<QuoteService>();
            builder.RegisterType<GiveawayService>();
            builder.RegisterType<TwitchInfoService>();

            // threads
            builder.RegisterType<FollowerSubscriberListener>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<TwitchChatterListener>();
            builder.RegisterType<BankHeist>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
            builder.RegisterType<BossFight>()
                .WithParameter(ConnectionString)
                .WithParameter(TwitchBotApiLink);
        }
    }
}
