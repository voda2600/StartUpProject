using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var token = await _authService.RegisterAsync(request);
        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (token, isConfirmed) = await _authService.LoginAsync(request);
        return Ok(new { Token = token, IsEmailConfirmed = isConfirmed });
    }

    [HttpPost("confirm-email-code")]
    public async Task<IActionResult> ConfirmEmailCode([FromBody] ConfirmEmailRequest request)
    {
        var isConfirmed = await _authService.ConfirmEmailCode(request);
        return isConfirmed ? Ok() : Conflict("Code is invalid or expired");
    }

    [HttpPost("update-email-code")]
    public async Task<IActionResult> UpdateEmailCode([FromBody] UpdateCodeRequest request)
    {
        await _authService.UpdateEmailCode(request);
        return Ok();
    }
}