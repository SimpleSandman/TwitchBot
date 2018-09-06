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
        private readonly SimpleBotContext _context;

        public PartyUpRequestsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/partyuprequests/get/2?username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int partyMemberId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyUpRequest partyUpRequests = await _context.PartyUpRequest.SingleOrDefaultAsync(m => m.PartyMember == partyMemberId && m.Username == username);

            if (partyUpRequests == null)
            {
                return NotFound();
            }

            return Ok(partyUpRequests);
        }

        // POST: api/partyuprequests/create
        // Body (JSON): { "username": "hello_world", "partyMember": 2 }
        [HttpPost("{broadcasterId:int}")]
        public async Task<IActionResult> Create([FromBody] PartyUpRequest partyUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (PartyUpRequestExists(partyUpRequest.Username, partyUpRequest.PartyMember))
            {
                return BadRequest();
            }

            _context.PartyUpRequest.Add(partyUpRequest);
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

            PartyUpRequest requestToBeDeleted = await _context.PartyUpRequest
                //.Where(m => m.Broadcaster == broadcasterId && m.Game == gameId)
                .OrderBy(m => m.TimeRequested)
                .Take(1)
                .SingleOrDefaultAsync();

            if (requestToBeDeleted == null)
            {
                return NotFound();
            }

            _context.PartyUpRequest.Remove(requestToBeDeleted);
            await _context.SaveChangesAsync();

            return Ok(requestToBeDeleted);
        }

        private bool PartyUpRequestExists(string username, int partyMemberId)
        {
            return _context.PartyUpRequest.Any(e => e.Username == username && e.PartyMember == partyMemberId);
        }
    }
}