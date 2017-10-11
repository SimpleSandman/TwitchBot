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
        public Autofac.Core.Parameter ParamConnStr { get; set; }
        public TwitchBotConfigurationSection TwitchBotConfigurationSection { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // configuration
            builder.RegisterInstance(AppConfig);
            builder.RegisterInstance(TwitchBotConfigurationSection);

            // main app
            builder.RegisterType<TwitchBotApplication>();

            // repositories
            builder.RegisterType<BankRepository>().WithParameter(ParamConnStr);
            builder.RegisterType<FollowerRepository>().WithParameter(ParamConnStr);
            builder.RegisterType<SongRequestBlacklistRepository>().WithParameter(ParamConnStr);
            builder.RegisterType<ManualSongRequestRepository>().WithParameter(ParamConnStr);
            builder.RegisterType<PartyUpRepository>().WithParameter(ParamConnStr);
            builder.RegisterType<GameDirectoryRepository>().WithParameter(ParamConnStr);
            builder.RegisterType<QuoteRepository>().WithParameter(ParamConnStr);
            builder.RegisterType<GiveawayRepository>().WithParameter(ParamConnStr);

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
            builder.RegisterType<FollowerListener>().WithParameter(ParamConnStr);
            builder.RegisterType<BankHeist>().WithParameter(ParamConnStr);
        }
    }
}
