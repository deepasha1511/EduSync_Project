using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EduSync.Backend.Controllers;
using EduSync.Backend.Data;
using EduSync.Backend.DTOs;
using EduSync.Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EduSync.Tests.Controllers
{
    public class AuthControllerTests : IDisposable
    {
        private readonly EduSyncDbContext _context;
        private readonly AuthController _controller;
        private readonly IConfiguration _configuration;

        private readonly PasswordHasher<User> _hasher = new();

        private readonly DbContextOptions<EduSyncDbContext> _options;

        public AuthControllerTests()
        {
            // In-memory EF Core DB for testing
            _options = new DbContextOptionsBuilder<EduSyncDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EduSyncDbContext(_options);

            // Setup config mock (for JWT key, issuer, audience)
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "supersecretkey_supersecretkey1234"}, // min length for HMACSHA256
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _controller = new AuthController(_context, _configuration);
        }

        [Fact]
        public async Task Register_CreatesNewUser_ReturnsOk()
        {
            var dto = new CreateUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                Role = "Student"
            };

            var result = await _controller.Register(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonSerializer.Serialize(okResult.Value);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            Assert.NotNull(deserialized);
            Assert.Equal("User registered successfully", deserialized["message"].ToString());
            Assert.True(Guid.TryParse(deserialized["UserId"].ToString(), out _));  // ← fix here
        }


        [Fact]
        public void Login_WithValidCredentials_ReturnsTokenAndRole()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = "Login User",
                Email = "login@example.com",
                Role = "Student"
            };
            user.PasswordHash = _hasher.HashPassword(user, "Password123!");
            _context.Users.Add(user);
            _context.SaveChanges();

            var dto = new LoginUserDto
            {
                Email = user.Email,
                Password = "Password123!"
            };

            var result = _controller.Login(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            Assert.NotNull(deserialized);
            Assert.True(deserialized.ContainsKey("token"));
            Assert.Equal(user.Role, deserialized["role"].ToString());

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(deserialized["token"].ToString());

            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.UserId.ToString());
        }


        [Fact]
        public void Login_WithInvalidEmail_ReturnsUnauthorized()
        {
            var dto = new LoginUserDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            var result = _controller.Login(dto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not found", unauthorizedResult.Value);
        }

        [Fact]
        public void Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = "Login User",
                Email = "login@example.com",
                Role = "Student"
            };
            user.PasswordHash = _hasher.HashPassword(user, "Password123!");
            _context.Users.Add(user);
            _context.SaveChanges();

            var dto = new LoginUserDto
            {
                Email = user.Email,
                Password = "WrongPassword!"
            };

            var result = _controller.Login(dto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid password", unauthorizedResult.Value);
        }

        [Fact]
        public void Login_WithoutJwtKeyConfigured_ReturnsServerError()
        {
            // Arrange controller with empty JWT key config
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Jwt:Key", ""},
                    {"Jwt:Issuer", "issuer"},
                    {"Jwt:Audience", "audience"}
                }).Build();

            var controller = new AuthController(_context, config);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = "User",
                Email = "user@example.com",
                Role = "Student"
            };
            user.PasswordHash = _hasher.HashPassword(user, "Password123!");
            _context.Users.Add(user);
            _context.SaveChanges();

            var dto = new LoginUserDto
            {
                Email = user.Email,
                Password = "Password123!"
            };

            var result = controller.Login(dto);

            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);
            Assert.Equal("JWT Key is not configured", serverErrorResult.Value);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
