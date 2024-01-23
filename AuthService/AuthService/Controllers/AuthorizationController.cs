using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(ILogger<AuthorizationController> logger)
    {
        _logger = logger;
    }

    [HttpGet(nameof(Authorize))]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public string Authorize()
    {
        return "Альберт лох";
    }

    [HttpGet(nameof(Privacy))]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public string Privacy()
    {
        return "HOHO";
    }
}