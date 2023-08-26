using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using aTES.Auth.ActionFilters;
using aTES.Auth.Data;
using aTES.Auth.Kafka.Models;
using aTES.Auth.Models;
using aTES.Auth.Models.Dtos;
using aTES.Common;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace aTES.Auth.Controllers;

[Route("api/userauthentication")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly KafkaDependentProducer<string, string> _producer;

    public AuthController(ApplicationDbContext context, IConfiguration configuration,
        KafkaDependentProducer<string, string> producer)
    {
        _context = context;
        _configuration = configuration;
        _producer = producer;
    }

    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    [ProducesResponseType(typeof(PopugUserCreatedModel), 201)]
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

        var popugStreamModel = new PopugUserStreamingModel()
        {
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            PublicId = user.PublicId
        };

        _context.Produce("stream-user-lifecycle",
            new Message<string, string>()
                { Value = BasePayload<PopugUserStreamingModel>.Create("stream.user.changed.v1", popugStreamModel).ToJson() });


        var popugCreatedModel = new PopugUserCreatedModel()
        {
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            PublicId = user.PublicId
        };

        _context.Produce("be-user-created",
            new Message<string, string>()
                { Value = BasePayload<PopugUserCreatedModel>.Create("user.created.v1", popugCreatedModel).ToJson() });

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

        var popugStreamModel = new PopugUserStreamingModel()
        {
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            PublicId = user.PublicId
        };

        _context.Produce("stream-user-lifecycle",
            new Message<string, string>()
                { Value = BasePayload<PopugUserStreamingModel>.Create("stream.user.changed.v1", popugStreamModel).ToJson() });

        await _context.SaveChangesAsync();

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

    [HttpPost("syncStreaming")]
    public async Task Sync()
    {
        var users = _context.Users.AsNoTracking().ToList();
        foreach (var user in users)
        {
            var popugStreamModel = new PopugUserStreamingModel()
            {
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                PublicId = user.PublicId
            };

            _producer.Produce("stream-user-lifecycle",
                new Message<string, string>()
                    { Value = BasePayload<PopugUserStreamingModel>.Create("stream.user.changed.v1", popugStreamModel).ToJson() });
        }
    }
}