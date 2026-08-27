using System;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, IToken tokenService) : BaseApiController

{
    [HttpPost("register")] //account/register
    public async Task<ActionResult<UserDto>> Register(Registerdto registerDto)
    {

        var username = registerDto.Username.Trim().ToLowerInvariant();
        if (await UserExists(username)) return BadRequest("Username is already taken");

        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            UserName = username,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key,
            DateOfbirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)),
            Gender = "Unknown",
            City = string.Empty,
            Country = string.Empty
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };

    }

    [HttpPost("Login")]

    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {

        var user = await context.Users
        .Include(p => p.Photos)
        .FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

        if (user == null) return Unauthorized("Invalid User Name");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if (user.PasswordHash.Length != computedHash.Length ||
            !CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            return Unauthorized("invalid Password");
        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
    }
}
