using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;

using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;

namespace TwitchBotShared.ClientLibraries
{
    // Reference: https://github.com/discord-net/Discord.Net/tree/dev/samples
    public class DiscordNetClient
    {
        private DiscordRestClient _restClient;
        private readonly TwitchBotConfigurationSection _botConfig;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public DiscordNetClient(TwitchBotConfigurationSection botSection)
        {
            _botConfig = botSection;
        }

        #region Public Methods
        public async Task<DateTime> ConnectAsync()
        {
            try
            {
                if (_restClient == null)
                {
                    _restClient = new DiscordRestClient();

                    // Subscribing to client events, so that we may receive them whenever they're invoked.
                    _restClient.Log += LogAsync;
                    _restClient.LoggedIn += LoggedIn;
                    _restClient.LoggedOut += LoggedOut;
                }

                if (_restClient.LoginState == LoginState.LoggedOut)
                {
                    await _restClient.LoginAsync(TokenType.Bot, _botConfig.DiscordToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordNetClient", "ConnectAsync()", false);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Add a Discord role to the requested user within a guild (Discord server)
        /// </summary>
        /// <param name="requestedUser">The requested Discord user with discriminator (#XXXX)</param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="guildName">The name of the Discord server</param>
        /// <returns></returns>
        public async Task AddRoleAsync(string requestedUser, int discriminator, string roleName, string guildName)
        {
            try
            {
                // Validate guild (Discord server)
                IReadOnlyCollection<RestGuild> guilds = await _restClient.GetGuildsAsync().ConfigureAwait(false);
                RestGuild guild = guilds.SingleOrDefault(g => g.Name == guildName);

                if (guild == null)
                {
                    Console.WriteLine($"Cannot find the Discord server, \"{guildName}\" that's tied to your Discord token");
                    return;
                }

                // Validate Discord user
                IEnumerable<RestGuildUser> users = await guild.GetUsersAsync().FlattenAsync().ConfigureAwait(false);
                RestGuildUser user = users.SingleOrDefault(u => u.Username == requestedUser && u.DiscriminatorValue == discriminator);

                if (user == null)
                {
                    Console.WriteLine($"Cannot find the user, \"{requestedUser}\" in the server, \"{guildName}\"");
                    return;
                }

                // Validate role
                RestRole role = guild.Roles.SingleOrDefault(r => r.Name == roleName);

                if (role == null)
                {
                    Console.WriteLine($"Cannot find the role, \"{roleName}\" in the server, \"{guildName}\"");
                    return;
                }

                await _restClient.AddRoleAsync(guild.Id, user.Id, role.Id).ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordNetClient", "AddRoleAsync(string, string, string)", false);
            }
        }

        public async Task LogOut()
        {
            try
            {
                await _restClient.LogoutAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordNetClient", "LogOut()", false);
            }
        }
        #endregion

        #region Private Methods
        private async Task<Task> LoggedOut()
        {
            if (_restClient != null)
            {
                await _restClient.DisposeAsync().ConfigureAwait(false);
            }

            Console.WriteLine($"{_restClient.CurrentUser.Username}#{_restClient.CurrentUser.Discriminator} has logged out of Discord");
            return Task.CompletedTask;
        }

        private Task LoggedIn()
        {
            Console.WriteLine($"Success! Your Discord bot, \"{_restClient.CurrentUser.Username}#{_restClient.CurrentUser.Discriminator}\", is available!");
            return Task.CompletedTask;
        }
        
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
        #endregion
    }
}
