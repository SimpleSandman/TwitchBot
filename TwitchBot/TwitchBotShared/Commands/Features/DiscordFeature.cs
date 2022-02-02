using System;
using System.Threading.Tasks;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Extensions;
using TwitchBotShared.Models;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Discord" feature
    /// </summary>
    public sealed class DiscordFeature : BaseFeature
    {
        private readonly DiscordNetClient _discord;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public DiscordFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, DiscordNetClient discord) : base(irc, botConfig)
        {
            _discord = discord;
            _rolePermissions.Add("!discordconnect", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!discordaddrole", new CommandPermission { General = ChatterType.Broadcaster });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!discordconnect": // Manually connect to Discord
                        return (true, await _discord.ConnectAsync());
                    case "!discordaddrole":
                        return (true, await AddRoleAsync(chatter));
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
                int firstSpaceIndex = twitchChatter.Message.IndexOf(' ');
                int discriminatorIndex = twitchChatter.Message.IndexOf('#');

                if (firstSpaceIndex == -1)
                {
                    _irc.SendPublicChatMessage($"You forgot to add a space in the command since this takes 2 arguments");
                    return DateTime.Now;
                }

                if (discriminatorIndex == -1)
                {
                    _irc.SendPublicChatMessage($"You need the discriminator (#) in the Discord username");
                    return DateTime.Now;
                }

                if (!int.TryParse(twitchChatter.Message.AsSpan(twitchChatter.Message.IndexOf('#') + 1, 4), out int discriminator))
                {
                    _irc.SendPublicChatMessage($"The discriminator (#XXXX) for the requested Discord name was not found");
                    return DateTime.Now;
                }

                if (discriminator.Digits() != 4)
                {
                    _irc.SendPublicChatMessage($"The discriminator (#XXXX) isn't 4-digits long");
                    return DateTime.Now;
                }

                int startingRoleIndex = discriminatorIndex + 6; // compensate for the 4 numbers after the #

                string requestedUser = twitchChatter.Message.Substring(firstSpaceIndex + 1, discriminatorIndex - firstSpaceIndex - 1);
                string roleName = twitchChatter.Message.Substring(startingRoleIndex);

                await _discord.AddRoleAsync(requestedUser, discriminator, roleName, _botConfig.DiscordServerName);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordFeature", "AddRoleAsync(TwitchChatter)", false, "!discordaddrole", twitchChatter.Message);
            }

            return DateTime.Now;
        }
    }
}
