using System.Threading.Tasks;

using TwitchBotDb.Models;

namespace TwitchBotDb.Repositories
{
    public class DiscordSelfAssignRoleRepository
    {
        private readonly string _twitchBotApiLink;

        public DiscordSelfAssignRoleRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<DiscordSelfRoleAssign> GetDiscordRoleAsync(int broadcasterId, string serverName, string roleName)
        {
            return await ApiBotRequest.GetExecuteAsync<DiscordSelfRoleAssign>(_twitchBotApiLink 
                + $"discordselfroleassign/get/{broadcasterId}?servername={serverName}&rolename={roleName}");
        }
    }
}
