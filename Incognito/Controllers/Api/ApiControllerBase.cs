using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace IncognitoPAS.Controllers.Api;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string GetCurrentUserId()
    {
        // DECISION: Support both mapped NameIdentifier and raw JWT sub claim for robustness.
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new UnauthorizedAccessException("Authenticated user id is missing.");
    }

    protected string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
