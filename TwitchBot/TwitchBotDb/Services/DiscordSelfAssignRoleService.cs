using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;

namespace TwitchBotDb.Services
{
    public class DiscordSelfAssignRoleService
    {
        private readonly DiscordSelfAssignRoleRepository _discordSelfAssignRoleDb;

        public DiscordSelfAssignRoleService(DiscordSelfAssignRoleRepository discordSelfAssignRoleDb)
        {
            _discordSelfAssignRoleDb = discordSelfAssignRoleDb;
        }

        public async Task<DiscordSelfRoleAssign> GetDiscordRoleAsync(int broadcasterId, string serverName, string roleName)
        {
            return await _discordSelfAssignRoleDb.GetDiscordRoleAsync(broadcasterId, serverName, roleName);
        }
    }
}
