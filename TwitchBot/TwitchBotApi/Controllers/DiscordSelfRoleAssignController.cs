using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class DiscordSelfRoleAssignController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public DiscordSelfRoleAssignController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/discordselfroleassign/get/2?servername=nightlyrng&rolename=minecraft
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] string serverName, [FromQuery] string roleName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DiscordSelfRoleAssign role = await _context.DiscordSelfRoleAssigns
                .FirstOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.ServerName == serverName && m.RoleName == roleName);

            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }
    }
}