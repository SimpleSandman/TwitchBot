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
    public class PartyUpRequestsController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public PartyUpRequestsController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/partyuprequests/get/2
        // GET: api/partyuprequests/get/2?gameId=1
        // GET: api/partyuprequests/get/2?gameId=1&username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] int gameId = 0, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var partyUpRequests = new object();

            if (gameId > 0 && !string.IsNullOrEmpty(username))
                partyUpRequests = await _context.PartyUpRequests.SingleOrDefaultAsync(m => m.Broadcaster == broadcasterId && m.Game == gameId && m.Username == username);
            else if (gameId > 0)
                partyUpRequests = await _context.PartyUpRequests.Where(m => m.Broadcaster == broadcasterId && m.Game == gameId).ToListAsync();
            else
                partyUpRequests = await _context.PartyUpRequests.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

            if (partyUpRequests == null)
            {
                return NotFound();
            }

            return Ok(partyUpRequests);
        }

        // POST: api/partyuprequests/create/2
        // Body (JSON): { "username": "hello_world", "partyMember": "Sinon", "broadcaster": 2, "game": 2 }
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