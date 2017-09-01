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

            // services
            builder.RegisterType<BankService>();
            builder.RegisterType<FollowerService>();
            builder.RegisterType<SongRequestBlacklistService>();
            builder.RegisterType<TwitchInfoService>();

            // threads
            builder.RegisterType<FollowerListener>().WithParameter(ParamConnStr);
        }
    }
}
