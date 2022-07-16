using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Snickler.EFCore;

using TwitchBot.Api.Helpers;

using TwitchBotDb.Context;
using TwitchBotDb.DTO;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PartyUpRequestsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public PartyUpRequestsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/partyuprequests/getuserrequest/2?username=simple_sandman
        [HttpGet("{partyMemberId}")]
        public async Task<IActionResult> GetUserRequest(int partyMemberId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyUpRequest? partyUpRequests = await _context.PartyUpRequests
                .SingleOrDefaultAsync(m => m.PartyMemberId == partyMemberId && m.Username == username);

            if (partyUpRequests == null)
            {
                throw new NotFoundException("Party up request cannot be found");
            }

            return Ok(partyUpRequests);
        }

        // GET: api/partyuprequests/getlist/2?gameid=2
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> GetList(int broadcasterId, [FromQuery] int gameId)
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
                throw new ApiException("Party up request already created");
            }

            _context.PartyUpRequests.Add(partyUpRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/partyuprequests/deletefirst/2?gameid=2
        [HttpDelete("{broadcasterId}")]
        public async Task<IActionResult> DeleteFirst(int broadcasterId, [FromQuery] int gameId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyUpRequestResult? result = new PartyUpRequestResult();

            await _context.LoadStoredProc("dbo.GetPartyUpRequestList")
                .WithSqlParam("GameId", gameId)
                .WithSqlParam("BroadcasterId", broadcasterId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    result = handler.ReadToList<PartyUpRequestResult>().FirstOrDefault();
                });

            if (result == null)
            {
                throw new NotFoundException("Party up request cannot be found for deletion");
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
                throw new NotFoundException("Party up request cannot be found for deletion");
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
