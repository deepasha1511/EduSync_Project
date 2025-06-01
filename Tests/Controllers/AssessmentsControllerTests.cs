using Microsoft.EntityFrameworkCore;
using EduSync.Backend.Controllers;
using EduSync.Backend.Data;
using EduSync.Backend.Models;
using EduSync.Backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.Tests.Controllers
{
    public class AssessmentsControllerTests
    {
        private readonly EduSyncDbContext _context;
        private readonly AssessmentsController _controller;

        public AssessmentsControllerTests()
        {
            // Setup in-memory DbContext
            var options = new DbContextOptionsBuilder<EduSyncDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EduSyncDbContext(options);
            _controller = new AssessmentsController(_context);

            // Seed test data
            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var courseId = Guid.NewGuid();
            var course = new Course
            {
                CourseId = courseId,
                Title = "Unit Test Course",
                Description = "Test description",
                InstructorId = Guid.NewGuid()
            };

            var assessments = new List<Assessment>
        {
            new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                CourseId = courseId,
                Title = "Test Assessment 1",
                MaxScore = 100,
                Questions = "[{\"question\":\"Q1\",\"options\":[\"A\",\"B\"],\"answer\":\"A\"}]"

            },
            new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                CourseId = courseId,
                Title = "Test Assessment 2",
                MaxScore = 100,
                Questions = "[{\"question\":\"Q2\",\"options\":[\"A\",\"B\"],\"answer\":\"B\"}]"
            }
        };

            await _context.Courses.AddAsync(course);
            await _context.Assessments.AddRangeAsync(assessments);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAssessments_ReturnsAssessmentsForCourse()
        {
            // Arrange
            var course = await _context.Courses.FirstAsync();
            var expectedCount = await _context.Assessments.CountAsync(a => a.CourseId == course.CourseId);

            // Act
            var response = await _controller.GetAssessments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var allAssessments = Assert.IsType<List<AssessmentDto>>(okResult.Value);
            var courseAssessments = allAssessments.Where(a => a.CourseId == course.CourseId).ToList();

            Assert.Equal(expectedCount, courseAssessments.Count);
            Assert.All(courseAssessments, a => Assert.Equal(course.CourseId, a.CourseId));
        }
        [Fact]
        public async Task GetAssessment_ReturnsCorrectAssessment()
        {
            // Arrange
            var existing = await _context.Assessments.Include(a => a.Course).FirstAsync();

            // Act
            var result = await _controller.GetAssessment(existing.AssessmentId);

            // Assert
            var dto = Assert.IsType<AssessmentDto>(result.Value);
            Assert.Equal(existing.Title, dto.Title);
            Assert.Equal(existing.CourseId, dto.CourseId);
        }

        [Fact]
        public async Task GetAssessment_ReturnsNotFound_WhenInvalidId()
        {
            // Act
            var result = await _controller.GetAssessment(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostAssessment_CreatesAssessment()
        {
            // Arrange
            var course = await _context.Courses.FirstAsync();
            var dto = new CreateAssessmentDto
            {
                CourseId = course.CourseId,
                Title = "New Quiz",
                MaxScore = 80,
                Questions = "[{\"question\":\"Q2\",\"options\":[\"A\",\"B\"],\"answer\":\"B\"}]"
            };
            // Act
            var result = await _controller.PostAssessment(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<AssessmentDto>(created.Value);
            Assert.Equal(dto.Title, returned.Title);
            Assert.Equal(dto.CourseId, returned.CourseId);
        }

        [Fact]
        public async Task PostAssessment_ReturnsBadRequest_WhenCourseInvalid()
        {
            var dto = new CreateAssessmentDto
            {
                CourseId = Guid.NewGuid(),
                Title = "Broken",
                MaxScore = 50,
                Questions = "[]"
            };

            var result = await _controller.PostAssessment(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("does not exist", bad.Value.ToString());
        }

        [Fact]
        public async Task PutAssessment_ReturnsNotFound_IfMissing()
        {
            var id = Guid.NewGuid();
            var fake = new Assessment { AssessmentId = id, CourseId = Guid.NewGuid(), Title = "X", MaxScore = 10, Questions = "[]" };

            var result = await _controller.PutAssessment(id, fake);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAssessment_RemovesIt()
        {
            var existing = await _context.Assessments.FirstAsync();
            var result = await _controller.DeleteAssessment(existing.AssessmentId);
            Assert.IsType<NoContentResult>(result);

            var gone = await _context.Assessments.FindAsync(existing.AssessmentId);
            Assert.Null(gone);
        }

    }
}