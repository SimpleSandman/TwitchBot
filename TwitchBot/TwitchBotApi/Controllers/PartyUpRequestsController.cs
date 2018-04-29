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
    public class PartyUpRequestsController : Controller
    {
        private readonly TwitchBotContext _context;

        public PartyUpRequestsController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/partyuprequests/get/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var partyUpRequests = await _context.PartyUpRequests.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

            if (partyUpRequests == null)
            {
                return NotFound();
            }

            return Ok(partyUpRequests);
        }

        // POST: api/partyuprequests/create/2
        [HttpPost("{broadcasterId:int}")]
        public async Task<IActionResult> Create([FromBody] PartyUpRequests partyUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (PartyUpRequestExists(partyUpRequest.Username, partyUpRequest.Broadcaster))
            {
                return BadRequest();
            }

            _context.PartyUpRequests.Add(partyUpRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/partyuprequests/deletetopone/2?gameid=2
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> DeleteTopOne([FromRoute] int broadcasterId, [FromQuery] int gameId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyUpRequests requestToBeDeleted = await _context.PartyUpRequests
                .Where(m => m.Broadcaster == broadcasterId && m.Game == gameId)
                .OrderBy(m => m.TimeRequested)
                .Take(1)
                .SingleOrDefaultAsync();

            if (requestToBeDeleted == null)
            {
                return NotFound();
            }

            _context.PartyUpRequests.Remove(requestToBeDeleted);
            await _context.SaveChangesAsync();

            return Ok(requestToBeDeleted);
        }

        private bool PartyUpRequestExists(string username, int broadcasterId)
        {
            return _context.PartyUpRequests.Any(e => e.Username == username && e.Broadcaster == broadcasterId);
        }
    }
}