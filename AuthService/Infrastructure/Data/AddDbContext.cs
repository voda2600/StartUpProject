﻿using AuthService.Domain.Enities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    //dotnet ef migrations add CreateAuthDatabase --context AppDbContext --project C:\Users\Albert\Desktop\StartUpProject\AuthService\AuthService\AuthService\AuthService.csproj
    //dotnet ef database update --context AppDbContext
}