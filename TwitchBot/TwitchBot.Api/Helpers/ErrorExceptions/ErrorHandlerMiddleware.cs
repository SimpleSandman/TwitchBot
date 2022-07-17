using System.Net;
using System.Text.Json;

namespace TwitchBot.Api.Helpers.ErrorExceptions
{
    // Reference: https://jasonwatmore.com/post/2022/03/15/net-6-crud-api-example-and-tutorial
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                HttpResponse? response = context.Response;
                response.ContentType = "application/json";

                switch (error)
                {
                    case ApiException:
                        // api exception (400)
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case NotFoundException:
                        // cannot find (404)
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error (500)
                        _logger.LogError(error, error.Message);
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                string? result = JsonSerializer.Serialize(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
