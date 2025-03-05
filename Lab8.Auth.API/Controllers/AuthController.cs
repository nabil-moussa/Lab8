using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lab8.Auth.Application.Services;
using Lab8.Auth.Common.Dtos;
using Lab8.Auth.Common.RabbitMQ;
using Lab8.Auth.Infrastructure.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Lab8.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;
    private readonly RabbitMQPublisher _rabbitMQPublisher;

    public AuthController(
        UserService userService, 
        IConfiguration configuration,
        RabbitMQPublisher rabbitMQPublisher)
    {
        _userService = userService;
        _configuration = configuration;
        _rabbitMQPublisher = rabbitMQPublisher;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try 
        {
            var user = await _userService.AuthenticateAsync(
                loginDto.Username, 
                loginDto.Password
            );

            var token = GenerateJwtToken(user);
            _rabbitMQPublisher.PublishUserEvent(new UserAuthenticatedEvent()
            {
                Username = user.Username,
                JwtToken = token,
                BranchId = user.BranchId,
                Role = user.Role
            }, "token.distributed");
            return Ok(new { Token = token });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto userDto)
    {
        var registeredUser = await _userService.RegisterUserAsync(userDto);
        
        // Publish user registration event
        _rabbitMQPublisher.PublishUserEvent(new {
            UserId = registeredUser.Id,
            Username = registeredUser.Username,
            Email = registeredUser.Email,
            BranchId = registeredUser.BranchId,
            Role = registeredUser.Role
        }, "user.registered");

        return Created("", registeredUser);
    }

    private string GenerateJwtToken(UserDto user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        );
        var credentials = new SigningCredentials(
            securityKey, 
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("BranchId", user.BranchId ?? "default"),
            
        };

        var token = new JwtSecurityToken(
            audience: _configuration["Jwt:Audience"],
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}