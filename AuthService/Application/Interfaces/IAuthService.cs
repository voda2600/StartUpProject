﻿using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequest request);

    Task<(string?, bool)> LoginAsync(LoginRequest request);

    Task<bool> ConfirmEmailCode(ConfirmEmailRequest request);

    Task UpdateEmailCode(UpdateCodeRequest request);
}