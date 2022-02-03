using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhitelistsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public WhitelistsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/Whitelists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Whitelist>>> GetWhitelist()
        {
            return await _context.Whitelists.ToListAsync();
        }

        // GET: api/Whitelists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Whitelist>> GetWhitelist(int id)
        {
            var whitelist = await _context.Whitelists.FindAsync(id);

            if (whitelist == null)
            {
                return NotFound();
            }

            return whitelist;
        }

        // PUT: api/Whitelists/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWhitelist(int id, Whitelist whitelist)
        {
            if (id != whitelist.Id)
            {
                return BadRequest();
            }

            _context.Entry(whitelist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WhitelistExists(id))
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

        // POST: api/Whitelists
        [HttpPost]
        public async Task<ActionResult<Whitelist>> PostWhitelist(Whitelist whitelist)
        {
            _context.Whitelists.Add(whitelist);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWhitelist", new { id = whitelist.Id }, whitelist);
        }

        // DELETE: api/Whitelists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Whitelist>> DeleteWhitelist(int id)
        {
            var whitelist = await _context.Whitelists.FindAsync(id);
            if (whitelist == null)
            {
                return NotFound();
            }

            _context.Whitelists.Remove(whitelist);
            await _context.SaveChangesAsync();

            return whitelist;
        }

        private bool WhitelistExists(int id)
        {
            return _context.Whitelists.Any(e => e.Id == id);
        }
    }
}
