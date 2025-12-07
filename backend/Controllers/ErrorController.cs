using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("error")]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [HttpPatch]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            if (exception != null)
            {
                _logger.LogError(exception, "Unhandled exception occurred.");

                return StatusCode(500, new
                {
                    error = "An error occurred.",
                    message = exception.Message,
                    detail = HttpContext.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
                        ? exception.ToString()
                        : null
                });
            }

            return StatusCode(500, new { error = "An unknown error occurred." });
        }
    }
}

