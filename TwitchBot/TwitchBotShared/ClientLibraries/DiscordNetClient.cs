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
        /// <param name="requestedUser">The requested Discord user</param>
        /// <param name="discriminator">The disciminator (#XXXX after the username)</param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="guildName">The name of the Discord server</param>
        /// <returns>Response to adding role to the server if successful; otherwise return an error message</returns>
        public async Task<string> AddRoleAsync(string requestedUser, string discriminator, string roleName, string guildName)
        {
            try
            {
                (RestGuild, RestGuildUser, RestRole, string) validInfo = 
                    await GetValidDiscordInfoAsync(requestedUser, discriminator, roleName, guildName);

                if (!string.IsNullOrEmpty(validInfo.Item4))
                {
                    if (validInfo.Item4 == "Unknown error")
                    {
                        return ""; // error has already been thrown
                    }

                    return validInfo.Item4; // send error message
                }

                await _restClient.AddRoleAsync(validInfo.Item1.Id, validInfo.Item2.Id, validInfo.Item3.Id).ConfigureAwait(false);
                return $"{requestedUser}#{discriminator} has the role, \"{roleName}\" for the discord server, \"{guildName}\"";
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordNetClient", "AddRoleAsync(string, string, string, string)", false);
            }

            return "";
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
        private async Task<(RestGuild, RestGuildUser, RestRole, string)> GetValidDiscordInfoAsync(string requestedUser, string discriminator, string roleName, string guildName)
        {
            try
            {
                // Validate guild (Discord server)
                IReadOnlyCollection<RestGuild> guilds = await _restClient.GetGuildsAsync().ConfigureAwait(false);
                RestGuild guild = guilds.SingleOrDefault(g => g.Name == guildName);

                if (guild == null)
                {
                    string errorMessage = $"Cannot find the Discord server, \"{guildName}\" that's tied to your Discord token";
                    return (null, null, null, errorMessage);
                }

                // Validate Discord user
                IEnumerable<RestGuildUser> users = await guild.GetUsersAsync().FlattenAsync().ConfigureAwait(false);
                RestGuildUser user = users.SingleOrDefault(u => u.Username == requestedUser && u.Discriminator == discriminator);

                if (user == null)
                {
                    string errorMessage = $"Cannot find the user, \"{requestedUser}\" in the server, \"{guildName}\"";
                    return (null, null, null, errorMessage);
                }

                // Validate role
                RestRole role = guild.Roles.SingleOrDefault(r => r.Name == roleName);

                if (role == null)
                {
                    string errorMessage = $"Cannot find the role, \"{roleName}\" in the server, \"{guildName}\"";
                    return (null, null, null, errorMessage);
                }

                return (guild, user, role, "");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "DiscordNetClient", "GetValidDiscordInfoAsync(string, int, string, string)", false);
            }

            return (null, null, null, "Unknown error");
        }

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
