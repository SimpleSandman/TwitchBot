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
    public class RanksController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public RanksController(TwitchBotDbContext context)
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

            List<Rank> rank = await _context.Rank.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

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

            if (id != rank.Id || broadcasterId != rank.Broadcaster)
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

        // POST: api/ranks/create
        // Body (JSON): { "name": "New Rank", "expCap": 24, "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Rank rank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Rank.Add(rank);
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

            Rank rank = await _context.Rank.SingleOrDefaultAsync(m => m.Id == id && m.Broadcaster == broadcasterId);
            if (rank == null)
            {
                return NotFound();
            }

            _context.Rank.Remove(rank);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RankExists(int id)
        {
            return _context.Rank.Any(e => e.Id == id);
        }
    }
}