using Microsoft.AspNetCore.Mvc;

namespace TwitchBot.Api.Helpers
{
    public class ExtendedControllerBase : ControllerBase
    {
        /// <summary>
        /// Gets a value that indicates whether any model state values in this model state dictionary is 
        /// invalid or not validated.
        /// </summary>
        /// <returns>Produces a StatusCodes.Status400BadRequest response if ModelState is not valid; 
        /// otherwise return null</returns>
        protected BadRequestObjectResult? IsModelStateValid()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
