using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class HealthController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            application = "Kron.Counting.API",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            timestampUtc = DateTime.UtcNow
        });
    }
}

//using Microsoft.AspNetCore.Mvc;
//using Kron.Counting.Shared.Exceptions;

//namespace Kron.Counting.API.Controllers;

//[ApiController]
//[Route("api/v1/[controller]")]
//public sealed class HealthController : ControllerBase
//{
//    [HttpGet]
//    public IActionResult Get()
//    {
//        throw new NotFoundException("Health middleware test");
//    }
//}

