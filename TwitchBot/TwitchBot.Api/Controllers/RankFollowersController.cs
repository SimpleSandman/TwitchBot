using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers.ErrorExceptions;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RankFollowersController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public RankFollowersController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/rankfollowers/get/2?username=simple_sandman
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RankFollower? rankFollower = await _context.RankFollowers
                .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Username == username);

            if (rankFollower == null)
            {
                return Ok(new RankFollower { Username = username, Experience = -1, BroadcasterId = broadcasterId });
            }

            return Ok(rankFollower);
        }

        // GET: api/rankfollowers/getleaderboard/2?topnumber=5
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> GetLeaderboard(int broadcasterId, [FromQuery] int topNumber = 3)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IEnumerable<RankFollower> topFollowers = await _context.RankFollowers
                .Where(m => m.BroadcasterId == broadcasterId)
                .OrderByDescending(m => m.Experience)
                .Take(topNumber)
                .ToListAsync();

            if (topFollowers == null || !topFollowers.Any())
            {
                throw new NotFoundException("Top followers cannot be found");
            }

            return Ok(topFollowers);
        }

        // PUT: api/rankfollowers/updateexp/2?username=simple_sandman&exp=9001
        [HttpPut("{broadcasterId}")]
        public async Task<IActionResult> UpdateExp(int broadcasterId, [FromQuery] string username, [FromQuery] int exp)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RankFollower? follower = await _context.RankFollowers
                .FirstOrDefaultAsync(t => t.BroadcasterId == broadcasterId && t.Username == username);

            if (follower == null)
            {
                throw new NotFoundException("Follower cannot be found");
            }

            follower.Experience = exp;
            _context.RankFollowers.Update(follower);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RankFollowersExists(broadcasterId, username))
                {
                    throw new NotFoundException("Follower cannot be found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/rankfollowers/create
        // Body (JSON): { "username": "simple_sandman", "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RankFollower rankFollowers)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.RankFollowers.Add(rankFollowers);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RankFollowersExists(int broadcasterId, string username)
        {
            return _context.RankFollowers.Any(e => e.BroadcasterId == broadcasterId && e.Username == username);
        }
    }
}
