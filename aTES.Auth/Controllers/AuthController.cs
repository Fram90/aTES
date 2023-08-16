using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using aTES.Auth.ActionFilters;
using aTES.Auth.Data;
using aTES.Auth.Kafka;
using aTES.Auth.Kafka.Models;
using aTES.Auth.Models;
using aTES.Auth.Models.Dtos;
using aTES.Common;
using Confluent.Kafka;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace aTES.Auth.Controllers;

[Route("api/userauthentication")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly KafkaDependentProducer<Null, string> _producer;

    public AuthController(ApplicationDbContext context, IConfiguration configuration,
        KafkaDependentProducer<Null, string> producer)
    {
        _context = context;
        _configuration = configuration;
        _producer = producer;
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

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
                                          {
                                              SqlState: PostgresErrorCodes.UniqueViolation
                                          })
        {
            return BadRequest("User with same email already exists");
        }

        var popugStreamModel = new PopugUserStreamingModel()
        {
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            PublicId = user.PublicId
        };

        await _producer.ProduceAsync("stream-user-lifecycle",
            new Message<Null, string>()
                { Value = BaseMessage<PopugUserStreamingModel>.Create(popugStreamModel).ToJson() });

        return Ok(user);
    }

    [HttpPost("changeRole")]
    [ValidationFilter]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto model)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == model.Email);
        if (user == null)
        {
            return NotFound();
        }

        user.Role = model.NewRole;
        await _context.SaveChangesAsync();

        var popugStreamModel = new PopugUserStreamingModel()
        {
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            PublicId = user.PublicId
        };
        
        await _producer.ProduceAsync("stream-user-lifecycle",
            new Message<Null, string>()
                { Value = BaseMessage<PopugUserStreamingModel>.Create(popugStreamModel).ToJson() });

        return Ok(user);
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
}