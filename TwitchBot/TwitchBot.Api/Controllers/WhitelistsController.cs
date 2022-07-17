using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers.ErrorExceptions;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
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
            Whitelist? whitelist = await _context.Whitelists.FindAsync(id);

            if (whitelist == null)
            {
                throw new NotFoundException("Whitelist cannot be found");
            }

            return whitelist;
        }

        // PUT: api/Whitelists/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWhitelist(int id, Whitelist whitelist)
        {
            if (id != whitelist.Id)
            {
                throw new ApiException("ID does not match whitelist's ID");
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
                    throw new NotFoundException("Whitelist cannot be found");
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
            Whitelist? whitelist = await _context.Whitelists.FindAsync(id);
            if (whitelist == null)
            {
                throw new NotFoundException("Whitelist cannot be found");
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
