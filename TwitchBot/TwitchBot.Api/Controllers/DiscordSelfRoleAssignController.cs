using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;
using TwitchBot.Api.Helpers.ErrorExceptions;
using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    public class DiscordSelfRoleAssignController : ExtendedControllerBase
    {
        private readonly SimpleBotContext _context;

        public DiscordSelfRoleAssignController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/discordselfroleassign/get/2?servername=nightlyrng&rolename=minecraft
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId, [FromQuery] string serverName, [FromQuery] string roleName)
        {
            IsModelStateValid();

            DiscordSelfRoleAssign? role = await _context.DiscordSelfRoleAssigns
                .FirstOrDefaultAsync(m => m.BroadcasterId == broadcasterId 
                    && m.ServerName == serverName 
                    && m.RoleName == roleName);

            if (role == null)
            {
                throw new NotFoundException("Discord self assign role does not exist");
            }

            return Ok(role);
        }
    }
}
