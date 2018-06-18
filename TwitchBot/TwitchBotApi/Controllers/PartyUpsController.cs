using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class PartyUpsController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public PartyUpsController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/partyups/get/2
        // GET: api/partyups/get/2?gameid=2
        // GET: api/partyups/get/2?gameid=2?partymember=Sinon
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] int gameId = 0, [FromQuery] string partyMember = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var partyUp = new object();

            if (gameId > 0 && !string.IsNullOrEmpty(partyMember))
            {
                partyUp = await _context.PartyUp
                    .SingleOrDefaultAsync(m =>
                        m.Broadcaster == broadcasterId
                            && m.Game == gameId
                            && m.PartyMember.Contains(partyMember, StringComparison.CurrentCultureIgnoreCase));
            }
            else if (gameId > 0)
            {
                partyUp = await _context.PartyUp.Where(m => m.Broadcaster == broadcasterId && m.Game == gameId)
                    .Select(m => m.PartyMember)
                    .ToListAsync();
            }
            else
            {
                partyUp = await _context.PartyUp.Where(m => m.Broadcaster == broadcasterId).ToListAsync();
            }

            if (partyUp == null)
            {
                return NotFound();
            }

            return Ok(partyUp);
        }
    }
}