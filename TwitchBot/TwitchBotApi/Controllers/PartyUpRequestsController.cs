using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Snickler.EFCore;

using TwitchBotDb.Context;
using TwitchBotDb.DTO;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class PartyUpRequestsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public PartyUpRequestsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/partyuprequests/get/2?username=simple_sandman
        [HttpGet("{partyMemberId:int}")]
        public async Task<IActionResult> GetUserRequest([FromRoute] int partyMemberId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyUpRequest partyUpRequests = await _context.PartyUpRequests.SingleOrDefaultAsync(m => m.PartyMemberId == partyMemberId && m.Username == username);

            if (partyUpRequests == null)
            {
                return NotFound();
            }

            return Ok(partyUpRequests);
        }

        // GET: api/partyuprequests/getlist/2?gameid=2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> GetList([FromRoute] int broadcasterId, [FromQuery] int gameId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<PartyUpRequestResult> results = new List<PartyUpRequestResult>();

            await _context.LoadStoredProc("dbo.GetPartyUpRequestList")
                .WithSqlParam("GameId", gameId)
                .WithSqlParam("BroadcasterId", broadcasterId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<PartyUpRequestResult>().ToList();
                });

            return Ok(results);
        }

        // POST: api/partyuprequests/create
        // Body (JSON): { "username": "hello_world", "partyMember": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartyUpRequest partyUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (PartyUpRequestExists(partyUpRequest.Username, partyUpRequest.PartyMemberId))
            {
                return BadRequest();
            }

            _context.PartyUpRequests.Add(partyUpRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/partyuprequests/deletefirst/2?gameid=2
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> DeleteFirst([FromRoute] int broadcasterId, [FromQuery] int gameId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyUpRequestResult result = new PartyUpRequestResult();

            await _context.LoadStoredProc("dbo.GetPartyUpRequestList")
                .WithSqlParam("GameId", gameId)
                .WithSqlParam("BroadcasterId", broadcasterId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    result = handler.ReadToList<PartyUpRequestResult>().FirstOrDefault();
                });

            if (result == null)
            {
                return NotFound();
            }

            PartyUpRequest requestToBeDeleted = new PartyUpRequest
            {
                Id = result.PartyRequestId,
                PartyMemberId = result.PartyMemberId,
                Username = result.Username,
                TimeRequested = result.TimeRequested
            };

            if (requestToBeDeleted == null)
            {
                return NotFound();
            }

            _context.PartyUpRequests.Remove(requestToBeDeleted);
            await _context.SaveChangesAsync();

            return Ok(result);
        }

        private bool PartyUpRequestExists(string username, int partyMemberId)
        {
            return _context.PartyUpRequests.Any(e => e.Username == username && e.PartyMemberId == partyMemberId);
        }
    }
}