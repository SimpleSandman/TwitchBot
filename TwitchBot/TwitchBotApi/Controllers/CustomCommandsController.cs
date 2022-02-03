using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class CustomCommandsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public CustomCommandsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/customcommands/get/2
        // GET: api/customcommands/get/2?name=!custom
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] string name = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customCommands = new object();

            if (string.IsNullOrEmpty(name))
                customCommands = await _context.CustomCommands.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();
            else
                customCommands = await _context.CustomCommands.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Name == name);

            if (customCommands == null)
            {
                return NotFound();
            }

            return Ok(customCommands);
        }

        // PUT: api/customcommands/5?broadcasterId=2
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromQuery] int broadcasterId, [FromBody] CustomCommand customCommand)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != customCommand.Id || broadcasterId != customCommand.BroadcasterId)
            {
                return BadRequest();
            }

            _context.Entry(customCommand).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomCommandExists(id))
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

        // POST: api/customcommands
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomCommand customCommand)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.CustomCommands.Add(customCommand);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = customCommand.Id }, customCommand);
        }

        // DELETE: api/customcommands/5?name=!custom
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customCommand = await _context.CustomCommands.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Name == name);
            if (customCommand == null)
            {
                return NotFound();
            }

            _context.CustomCommands.Remove(customCommand);
            await _context.SaveChangesAsync();

            return Ok(customCommand);
        }

        private bool CustomCommandExists(int id)
        {
            return _context.CustomCommands.Any(e => e.Id == id);
        }
    }
}
