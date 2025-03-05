using Lab8.Auth.Common.Dtos;
using Lab8.Auth.Domain.Models;
using Lab8.Auth.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Lab8.Auth.Application.Services;

public class UserService
{
    private readonly AuthDbContext _context;

    public UserService(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        // In real-world, use proper password hashing verification
        if (user.PasswordHash != password) 
            throw new UnauthorizedAccessException("Invalid credentials");

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            BranchId = user.BranchId
        };
    }

    public async Task<UserDto> RegisterUserAsync(UserDto userDto)
    {
        var user = new User
        {
            Id = Guid.NewGuid().GetHashCode(),
            Username = userDto.Username,
            Email = userDto.Email,
            Role = userDto.Role,
            BranchId = userDto.BranchId,
            PasswordHash = userDto.Password
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return userDto;
    }
}