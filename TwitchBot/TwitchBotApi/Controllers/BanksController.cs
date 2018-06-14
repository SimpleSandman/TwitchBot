using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Snickler.EFCore;

using TwitchBotApi.DTO;
using TwitchBotApi.Extensions;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class BanksController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public BanksController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/banks/get/2
        // GET: api/banks/get/2?username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Bank> bank = new List<Bank>();

            if (!string.IsNullOrEmpty(username))
                bank = await _context.Bank.Where(m => m.Broadcaster == broadcasterId && m.Username == username).ToListAsync();
            else
                bank = await _context.Bank.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

            if (bank == null || bank.Count == 0)
            {
                return NotFound();
            }

            return Ok(bank);
        }

        // PUT: api/banks/updateaccount/2?updatedwallet=5000&username=simple_sandman
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int broadcasterId, [FromQuery] int updatedWallet, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Bank bankAccount = _context.Bank.FirstOrDefault(t => t.Broadcaster == broadcasterId && t.Username == username);
            if (bankAccount == null)
            {
                return NotFound();
            }

            bankAccount.Wallet = updatedWallet;

            _context.Bank.Update(bankAccount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/banks/updatecreateaccount/2?deposit=5000&showOutput=true
        // Body (JSON): ["simple_sandman", "user1", "user2"]
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> UpdateCreateAccount([FromRoute] int broadcasterId, [FromQuery] int deposit, [FromQuery] bool showOutput, [FromBody] List<string> usernames)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<BalanceResult> results = new List<BalanceResult>();

            await _context.LoadStoredProc("dbo.UpdateCreateBalance")
                .WithSqlParam("tvpUsernames", usernames.ToDataTable())
                .WithSqlParam("intDeposit", deposit)
                .WithSqlParam("intBroadcasterID", broadcasterId)
                .WithSqlParam("bitShowOutput", showOutput)
                .ExecuteStoredProcAsync((handler) => 
                {
                    results = handler.ReadToList<BalanceResult>().ToList();
                });

            return Ok(results);
        }

        // POST: api/banks/createaccount
        // Body (JSON): { "username": "user1", "wallet": 500, "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] Bank bank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (BankExists(bank.Username, bank.Broadcaster))
            {
                return BadRequest();
            }

            _context.Bank.Add(bank);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/banks/getleaderboard/2?broadcastername=simple_sandman&botname=sandpaibot&topnumber=5
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> GetLeaderboard([FromRoute] int broadcasterId, [FromQuery] BroadcasterConfig broadcasterConfig, [FromQuery] int topNumber = 3)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Bank> bank = await _context.Bank
                .Where(m => m.Broadcaster == broadcasterId
                    && m.Username != broadcasterConfig.BroadcasterName
                    && m.Username != broadcasterConfig.BotName)
                .OrderByDescending(m => m.Wallet)
                .Take(topNumber)
                .ToListAsync();

            if (bank == null || bank.Count == 0)
            {
                return NotFound();
            }

            return Ok(bank);
        }

        private bool BankExists(string username, int broadcasterId)
        {
            return _context.Bank.Any(e => e.Username == username && e.Broadcaster == broadcasterId);
        }
    }
}