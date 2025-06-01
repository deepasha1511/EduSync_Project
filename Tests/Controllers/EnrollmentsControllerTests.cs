using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Backend.Controllers;
using EduSync.Backend.Data;
using EduSync.Backend.DTOs;
using EduSync.Backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduSync.Tests.Controllers
{
    public class EnrollmentsControllerTests
    {
        private readonly EduSyncDbContext _context;
        private readonly EnrollmentsController _controller;

        public EnrollmentsControllerTests()
        {
            var options = new DbContextOptionsBuilder<EduSyncDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EduSyncDbContext(options);
            _context.Database.EnsureCreated();
            _controller = new EnrollmentsController(_context);
        }

        [Fact]
        public async Task CreateEnrollment_WithValidData_ReturnsCreatedEnrollment()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = "Test Student",
                Email = "student@test.com",
                PasswordHash = "hashedpassword",
                Role = "Student"
            };
            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Title = "Test Course",
                Description = "Test Description"
            };
            await _context.Users.AddAsync(user);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            var enrollmentDto = new EnrollmentDto
            {
                UserId = user.UserId,
                CourseId = course.CourseId
            };

            // Act
            var result = await _controller.CreateEnrollment(enrollmentDto);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = createdAt.Value;
            Assert.NotNull(value);

            var serialized = System.Text.Json.JsonSerializer.Serialize(value);
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(serialized);

            Assert.Equal(user.UserId.ToString(), dict["UserId"].ToString());
            Assert.Equal(course.CourseId.ToString(), dict["CourseId"].ToString());
            Assert.Equal("Test Student", dict["UserName"].ToString());
            Assert.Equal("Test Course", dict["CourseTitle"].ToString());
        }

        [Fact]
        public async Task CreateEnrollment_WithNonStudentUser_ReturnsBadRequest()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = "Admin",
                Email = "admin@test.com",
                PasswordHash = "hashedpassword",
                Role = "Admin"
            };
            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Title = "Course",
                Description = "Description"
            };
            await _context.Users.AddAsync(user);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            var enrollmentDto = new EnrollmentDto
            {
                UserId = user.UserId,
                CourseId = course.CourseId
            };

            var result = await _controller.CreateEnrollment(enrollmentDto);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Equal("Only students can enroll in courses", badRequest.Value);
        }
     
        [Fact]

        public async Task MarkAsCompleted_WithValidEnrollment_UpdatesCompletionStatus()
        {
            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid(),
                CourseId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EnrollmentDate = DateTime.UtcNow,
                IsCompleted = false
            };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            var result = await _controller.MarkAsCompleted(enrollment.EnrollmentId);
            Assert.IsType<NoContentResult>(result);

            var updated = await _context.Enrollments.FindAsync(enrollment.EnrollmentId);
            Assert.True(updated.IsCompleted);
        }
    }
}
