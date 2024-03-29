﻿using Microsoft.AspNetCore.Mvc;

using TwitchBot.Api.Helpers;
using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ErrorLogsController : ExtendedControllerBase
    {
        private readonly SimpleBotContext _context;

        public ErrorLogsController(SimpleBotContext context)
        {
            _context = context;
        }

        // POST: api/errorlogs/create
        /* Body (JSON): 
            { 
              "errorTime": "2018-01-01T15:30:00",
              "errorLine": 9001,
              "errorClass": "SomeClass",
              "errorMethod": "SomeMethod",
              "errorMsg": "Some Error Message",
              "broadcaster": 2,
              "command": "!somecmd",
              "userMsg": "n/a"
            }
        */
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ErrorLog errorLog)
        {
            IsModelStateValid();

            _context.ErrorLogs.Add(errorLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
