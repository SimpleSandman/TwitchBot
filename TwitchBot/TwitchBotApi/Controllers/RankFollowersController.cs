using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotApi.DTO;

using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class RankFollowersController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public RankFollowersController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/rankfollowers/get/2?username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RankFollowers rankFollower = await _context.RankFollowers.SingleOrDefaultAsync(m => m.Broadcaster == broadcasterId && m.Username == username);

            if (rankFollower == null)
            {
                return Ok(new RankFollowers { Username = username, Exp = -1, Broadcaster = broadcasterId });
            }

            return Ok(rankFollower);
        }

        // GET: api/rankfollowers/getleaderboard/2?topnumber=5
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> GetLeaderboard([FromRoute] int broadcasterId, [FromQuery] int topNumber = 3)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IEnumerable<RankFollowers> topFollowers = await _context.RankFollowers
                .Where(m => m.Broadcaster == broadcasterId)
                .OrderByDescending(m => m.Exp)
                .Take(topNumber)
                .ToListAsync();

            if (topFollowers == null || topFollowers.Count() == 0)
            {
                return NotFound();
            }

            return Ok(topFollowers);
        }

        // PUT: api/rankfollowers/updateexp/2?username=simple_sandman&exp=9001
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> UpdateExp([FromRoute] int broadcasterId, [FromQuery] string username, [FromQuery] int exp)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RankFollowers follower = _context.RankFollowers.FirstOrDefault(t => t.Broadcaster == broadcasterId && t.Username == username);
            if (follower == null)
            {
                return NotFound();
            }

            follower.Exp = exp;
            _context.RankFollowers.Update(follower);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RankFollowersExists(broadcasterId, username))
                {
                    return NotFound();
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
        public async Task<IActionResult> Create([FromBody] RankFollowers rankFollowers)
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
            return _context.RankFollowers.Any(e => e.Broadcaster == broadcasterId && e.Username == username);
        }
    }
}