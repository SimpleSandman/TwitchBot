using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers.ErrorExceptions;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomCommandsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public CustomCommandsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/customcommands/get/2
        // GET: api/customcommands/get/2?name=!custom
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId, [FromQuery] string name = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object? customCommands = new object();

            if (string.IsNullOrEmpty(name))
            {
                customCommands = await _context.CustomCommands
                    .Where(m => m.BroadcasterId == broadcasterId)
                    .ToListAsync();
            }
            else
            {
                customCommands = await _context.CustomCommands
                    .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Name == name);
            }

            if (customCommands == null)
            {
                throw new NotFoundException("Custom commands not found");
            }

            return Ok(customCommands);
        }

        // PUT: api/customcommands/5?broadcasterId=2
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromQuery] int broadcasterId, [FromBody] CustomCommand customCommand)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != customCommand.Id || broadcasterId != customCommand.BroadcasterId)
            {
                throw new ApiException("ID or broadcaster ID does not match custom command ID or broadcaster ID");
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
                    throw new NotFoundException("Custom commands not found");
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
        [HttpDelete("{broadcasterId}")]
        public async Task<IActionResult> Delete(int broadcasterId, [FromQuery] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CustomCommand? customCommand = await _context.CustomCommands
                .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Name == name);

            if (customCommand == null)
            {
                throw new NotFoundException("Custom commands not found");
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
