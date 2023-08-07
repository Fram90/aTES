using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using aTES.Auth.ActionFilters;
using aTES.Auth.Data;
using aTES.Auth.Models;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace aTES.Auth.Controllers;

[Route("api/userauthentication")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    [ProducesResponseType(201)]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto userRegistration)
    {
        var user = new PopugUser()
        {
            PublicId = Guid.NewGuid(),
            Name = userRegistration.UserName,
            Role = userRegistration.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegistration.Password),
            Email = userRegistration.Email
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        //todo тут публикуем событие user created

        return StatusCode(201);
    }

    [HttpPost("changeRole")]
    [ValidationFilter]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto model)
    {
        var user = _context.Users.FirstOrDefault(x => x.Name == model.UserName);
        if (user == null)
        {
            return Unauthorized();
        }

        await _context.SaveChangesAsync();

        //todo тут публикуем событие userChanged
        return StatusCode(201);
    }

    [HttpPost("authenticate")]
    [ValidationFilter]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> Authenticate([FromBody] UserLoginDto loginModel)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == loginModel.Email);
        if (user == null)
        {
            return Unauthorized();
        }

        if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha512Signature
        );

        var subject = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Sub, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.ToString()),
            new Claim("publicId", user.PublicId.ToString())
        });

        var expires = DateTime.UtcNow.AddHours(8);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return Ok(jwtToken);
    }

    [HttpGet("TestApiAuth")]
    [Authorize]
    public async Task<IActionResult> TestApiAuth()
    {
        var user = User.Identity;
        return NoContent();
    }
}