using System;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Discord" feature
    /// </summary>
    public sealed class DiscordFeature : BaseFeature
    {
        private readonly DiscordNetClient _discordClient;
        private readonly DiscordSelfAssignRoleService _discordService;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        private const string DISCORD_CONNECT = "!discordconnect";
        private const string DISCORD_ADD_ROLE = "!discordaddrole";
        private const string DISCORD_SELF_ROLE = "!discordselfrole";

        public DiscordFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, DiscordNetClient discordClient, DiscordSelfAssignRoleService discordService)
            : base(irc, botConfig)
        {
            _discordClient = discordClient;
            _discordService = discordService;
            _rolePermissions.Add(DISCORD_CONNECT, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(DISCORD_ADD_ROLE, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(DISCORD_SELF_ROLE, new CommandPermission { General = ChatterType.Follower });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case DISCORD_CONNECT: // Manually connect to Discord
                        return (true, await _discordClient.ConnectAsync());
                    case DISCORD_ADD_ROLE: // Add Discord roles without checking if the user is a follower
                        return (true, await AddRoleAsync(chatter));
                    case DISCORD_SELF_ROLE: // Add Discord roles to followed Twitch chatters
                        return (true, await AddSelfRoleAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Add a Discord role to the requested user within a guild (Discord server)
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public async Task<DateTime> AddRoleAsync(TwitchChatter twitchChatter)
        {
            try
            {
                // Provide command usage info
                if (twitchChatter.Message == DISCORD_ADD_ROLE)
                {
                    _irc.SendPublicChatMessage($"Usage: {DISCORD_ADD_ROLE} discord_username#0000 discord_role");
                    return DateTime.Now;
                }

                (string, string, string, string) parsedMessage = ParseMessage(twitchChatter);

                string responseMessage = await _discordClient.AddRoleAsync(parsedMessage.Item1, parsedMessage.Item2, parsedMessage.Item3, _botConfig.DiscordServerName);

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    _irc.SendPublicChatMessage(responseMessage);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordFeature", "AddRoleAsync(TwitchChatter)", false, DISCORD_ADD_ROLE, twitchChatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Add a Discord role to the requested user within a guild (Discord server)
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public async Task<DateTime> AddSelfRoleAsync(TwitchChatter twitchChatter)
        {
            try
            {
                if (twitchChatter.Username == _botConfig.Broadcaster.ToLower())
                {
                    _irc.SendPublicChatMessage($"{_botConfig.Broadcaster.ToLower()}...get off your lazy bum and get the role yourself Kappa");
                    return DateTime.Now;
                }

                // Provide command usage info
                if (twitchChatter.Message == DISCORD_SELF_ROLE)
                {
                    _irc.SendPublicChatMessage($"Usage: {DISCORD_SELF_ROLE} discord_username#0000 discord_role");
                    return DateTime.Now;
                }

                // Check how long the user has been following the channel
                twitchChatter.CreatedAt = _twitchChatterListInstance.TwitchFollowers.FirstOrDefault(c => c.Username == twitchChatter.Username).CreatedAt;

                if (twitchChatter.CreatedAt == null)
                {
                    _irc.SendPublicChatMessage($"Cannot find you in the Twitch following list {twitchChatter.DisplayName}. Please try again later");
                    return DateTime.Now;
                }

                // Make sure the message is legit
                (string, string, string, string) parsedMessage = ParseMessage(twitchChatter);

                if (!string.IsNullOrEmpty(parsedMessage.Item4))
                {
                    _irc.SendPublicChatMessage(parsedMessage.Item4);
                    return DateTime.Now;
                }

                // Go find the requested role from the configured server
                DiscordSelfRoleAssign discordRoleAssign = await _discordService.GetDiscordRoleAsync(_broadcasterInstance.DatabaseId, _botConfig.DiscordServerName, parsedMessage.Item3);

                if (discordRoleAssign == null)
                {
                    _irc.SendPublicChatMessage($"Cannot return database results for Discord self role assignment {twitchChatter.DisplayName}");
                    return DateTime.Now;
                }

                // Check if the user has been following for more than the set amount of hours
                if (twitchChatter.CreatedAt.Value > DateTime.UtcNow.AddHours(-discordRoleAssign.FollowAgeMinimumHour))
                {
                    _irc.SendPublicChatMessage($"You need to have been following this channel for {discordRoleAssign.FollowAgeMinimumHour} hours {twitchChatter.DisplayName}");
                    return DateTime.Now;
                }

                string responseMessage = await _discordClient.AddRoleAsync(parsedMessage.Item1, parsedMessage.Item2, parsedMessage.Item3, _botConfig.DiscordServerName);

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    _irc.SendPublicChatMessage(responseMessage);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordFeature", "AddSelfRoleAsync(TwitchChatter)", false, DISCORD_SELF_ROLE, twitchChatter.Message);
            }

            return DateTime.Now;
        }

        #region Private Method
        private (string, string, string, string) ParseMessage(TwitchChatter twitchChatter)
        {
            int firstSpaceIndex = twitchChatter.Message.IndexOf(' ');
            int discriminatorIndex = twitchChatter.Message.IndexOf('#');

            if (firstSpaceIndex == -1)
            {
                return ("", "", "", $"You forgot to add a space in the command since this takes 2 arguments {twitchChatter.DisplayName}");
            }

            if (discriminatorIndex == -1)
            {
                return ("", "", "", $"You need the discriminator (#) in the Discord username {twitchChatter.DisplayName}");
            }

            string discriminator = twitchChatter.Message.Substring(twitchChatter.Message.IndexOf('#') + 1, 4);
            if (!int.TryParse(discriminator, out int _))
            {
                return ("", "", "", $"The discriminator (#XXXX) for the requested Discord name was not found {twitchChatter.DisplayName}");
            }

            if (discriminator.Length != 4)
            {
                return ("", "", "", $"The discriminator (#XXXX) isn't 4-digits long {twitchChatter.DisplayName}");
            }

            int startingRoleIndex = discriminatorIndex + 6; // compensate for the 4 numbers after the #

            string requestedUser = twitchChatter.Message.Substring(firstSpaceIndex + 1, discriminatorIndex - firstSpaceIndex - 1);
            string roleName = twitchChatter.Message.Substring(startingRoleIndex);

            return (requestedUser, discriminator, roleName, "");
        }
        #endregion
    }
}
