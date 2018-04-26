using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchBotApi.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class PartyUpsController : Controller
    {
        private readonly TwitchBotContext _context;

        public PartyUpsController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/partyups/getparty/2?gameId=1
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> GetParty([FromRoute] int broadcasterId, [FromQuery] int gameId = 0)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<PartyUp> partyMembers = new List<PartyUp>();

            if (gameId == 0)
            {
                partyMembers = await _context.PartyUp
                    .Where(m => m.Broadcaster == broadcasterId)
                    .ToListAsync();
            }
            else
            {
                partyMembers = await _context.PartyUp
                    .Where(m => m.Broadcaster == broadcasterId && m.Game == gameId)
                    .ToListAsync();
            }

            if (partyMembers == null || partyMembers.Count == 0)
            {
                return NotFound();
            }

            return Ok(partyMembers);
        }
    }
}