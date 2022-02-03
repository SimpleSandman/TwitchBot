using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Snickler.EFCore;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class RanksController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public RanksController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/ranks/get/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rank> rank = await _context.Ranks.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();

            if (rank == null || rank.Count == 0)
            {
                return NotFound();
            }

            return Ok(rank);
        }

        // PUT: api/ranks/update/5?broadcasterId=2
        // Body (JSON): 
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromQuery] int broadcasterId, [FromBody] Rank rank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != rank.Id || broadcasterId != rank.BroadcasterId)
            {
                return BadRequest();
            }

            _context.Entry(rank).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RankExists(id))
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

        // POST: api/ranks/createdefault
        // Body (JSON): { "name": "New Rank", "expCap": 24, "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> CreateDefault([FromBody] List<Rank> rank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rank> results = new List<Rank>();

            await _context.LoadStoredProc("dbo.CreateDefaultRanks")
                .WithSqlParam("BroadcasterId", rank.First().BroadcasterId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<Rank>().ToList();
                });

            return Ok(results);
        }

        // POST: api/ranks/create
        // Body (JSON): { "name": "New Rank", "expCap": 24, "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Rank rank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Ranks.Add(rank);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ranks/delete/5?broadcasterId=2
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Rank rank = await _context.Ranks.SingleOrDefaultAsync(m => m.Id == id && m.BroadcasterId == broadcasterId);
            if (rank == null)
            {
                return NotFound();
            }

            _context.Ranks.Remove(rank);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RankExists(int id)
        {
            return _context.Ranks.Any(e => e.Id == id);
        }
    }
}