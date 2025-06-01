using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EduSync.Backend.Data;
using EduSync.Backend.Models;
using EduSync.Backend.Controllers;
using EduSync.Backend.DTOs;

namespace EduSync.Tests.Controllers
{
    public class CoursesControllerTests
    {
        private readonly CoursesController _controller;
        private readonly EduSyncDbContext _context;
        public CoursesControllerTests()
        {
            var options = new DbContextOptionsBuilder<EduSyncDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
                .Options;

            _context = new EduSyncDbContext(options);
            _controller = new CoursesController(_context);
        }

        [Fact]
        public async Task GetCourses_ReturnsAllCourses()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            _context.Courses.Add(new Course
            {
                CourseId = Guid.NewGuid(),
                Title = "Sample Course",
                Description = "Testing",
                InstructorId = instructorId
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCourses();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<CourseDto>>>(result);
            var courses = Assert.IsAssignableFrom<IEnumerable<CourseDto>>(okResult.Value);
            Assert.Single(courses);
        }

        [Fact]
        public async Task GetCourseById_ReturnsNotFound_WhenInvalidId()
        {
            // Act
            var result = await _controller.GetCourseById(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostCourse_CreatesCourse()
        {
            // Arrange
            var instructorId = Guid.NewGuid().ToString();

            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, instructorId),
                new Claim(ClaimTypes.Role, "Instructor")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = fakeUser }
            };
            var dto = new CreateCourseDto
            {
                Title = "New Course",
                Description = "New Description",
                InstructorId = Guid.NewGuid(),
                MediaUrl = "http://example.com/video"
            };

            // Act
            var result = await _controller.PostCourse(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var course = Assert.IsType<CourseDto>(created.Value);
            Assert.Equal(dto.Title, course.Title);
        }

        [Fact]
        public async Task GetCourseById_ReturnsNotFound_WhenCourseDoesNotExist()
        {
            // Act
            var result = await _controller.GetCourseById(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutCourse_UpdatesCourse()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var course = new Course
            {
                CourseId = courseId,
                Title = "Old Title",
                Description = "Old Desc",
                InstructorId = instructorId,
                MediaUrl = "url"
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var updated = new CourseUpdateDto
            {
                Title = "Updated Title",
                Description = "Updated Desc",
                MediaUrl = "url2"
            };

            // Act
            var result = await _controller.PutCourse(courseId, updated);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var check = await _context.Courses.FindAsync(courseId);
            Assert.Equal("Updated Title", check.Title);
            Assert.Equal("url2", check.MediaUrl);
        }

        [Fact]
        public async Task DeleteCourse_RemovesCourse()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _context.Courses.Add(new Course
            {
                CourseId = courseId,
                Title = "To Be Deleted",
                Description = "Trash",
                InstructorId = Guid.NewGuid()
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCourse(courseId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deleted = await _context.Courses.FindAsync(courseId);
            Assert.Null(deleted);
        }
    }
}