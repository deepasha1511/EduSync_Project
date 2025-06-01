using EduSync.Backend.Controllers;
using EduSync.Backend.Data;
using EduSync.Backend.DTOs;
using EduSync.Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace EduSync.Tests.Controllers
{
    public class ResultsControllerTests
    {
        private readonly DbContextOptions<EduSyncDbContext> _options;

        public ResultsControllerTests()
        {
            _options = new DbContextOptionsBuilder<EduSyncDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ClaimsPrincipal GetMockUser(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        }

        [Fact]
        public async Task SubmitResult_SavesResultForValidUser()
        {
            using var context = new EduSyncDbContext(_options);
            var userId = Guid.NewGuid();
            var assessment = new Assessment { AssessmentId = Guid.NewGuid(), Title = "Science" };
            context.Assessments.Add(assessment);
            await context.SaveChangesAsync();

            var submission = new QuizSubmissionDto
            {
                AssessmentId = assessment.AssessmentId,
                Score = 75,
                Answers = new List<SubmittedAnswerDto>
                {
                    new() { QuestionIndex = 0, Answer = "A" }
                }
            };

            var controller = new ResultsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = GetMockUser(userId) }
            };

            var result = await controller.SubmitResult(submission);
            Assert.IsType<OkResult>(result);

            var stored = context.Results.FirstOrDefault();
            Assert.NotNull(stored);
            Assert.Equal(userId, stored.UserId);
            Assert.Equal(75, stored.Score);
        }

        [Fact]
        public async Task SubmitResult_ReturnsBadRequest_WhenSubmissionIsInvalid()
        {
            using var context = new EduSyncDbContext(_options);
            var controller = new ResultsController(context);
            var result = await controller.SubmitResult(null);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid submission.", badRequest.Value);
        }

        [Fact]
        public async Task DeleteResult_RemovesExistingResult()
        {
            using var context = new EduSyncDbContext(_options);
            var result = new Result
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Score = 90
            };
            context.Results.Add(result);
            await context.SaveChangesAsync();

            var controller = new ResultsController(context);
            var response = await controller.DeleteResult(result.ResultId);

            Assert.IsType<NoContentResult>(response);
            Assert.Null(await context.Results.FindAsync(result.ResultId));
        }

        [Fact]
        public async Task DeleteResult_ReturnsNotFound_WhenResultDoesNotExist()
        {
            using var context = new EduSyncDbContext(_options);
            var controller = new ResultsController(context);
            var response = await controller.DeleteResult(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task GetResultDetail_ReturnsNotFound_WhenResultIsMissing()
        {
            using var context = new EduSyncDbContext(_options);
            var controller = new ResultsController(context);
            var result = await controller.GetResultDetail(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
