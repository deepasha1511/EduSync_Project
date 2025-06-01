using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduSync.Backend.Data;
using EduSync.Backend.Models;
using Microsoft.AspNetCore.Identity;
using EduSync.Backend.DTOs;

namespace EduSync.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EduSyncDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher;

        public AuthController(EduSyncDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _hasher = new PasswordHasher<User>();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null) return Unauthorized("User not found");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) return Unauthorized("Invalid password");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString())
            };

            var keyString = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
                return StatusCode(500, "JWT Key is not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = user.Role
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = _hasher.HashPassword(null, dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", user.UserId });
        }
    }
}