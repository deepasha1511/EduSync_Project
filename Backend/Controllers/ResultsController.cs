using Microsoft.AspNetCore.Mvc;
using EduSync.Backend.Data;
using EduSync.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using EduSync.Backend.DTOs;

namespace EduSync.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public ResultsController(EduSyncDbContext context)
        {
            _context = context;
        }

        // GET: api/Results/byAssessment/{assessmentId}
        [HttpGet("byAssessment/{assessmentId}")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultsByAssessment(Guid assessmentId)
        {
            var results = await _context.Results
                .Include(r => r.User)
                .Where(r => r.AssessmentId == assessmentId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    Score = r.Score,
                    SubmittedAnswers = r.SubmittedAnswers,
                    AttemptDate = r.AttemptDate
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDetailDto>> GetResultDetail(Guid id)
        {
            var result = await _context.Results
                .Include(r => r.Assessment)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound();

            var questionsJson = result.Assessment?.Questions ?? "[]";
            var submittedJson = result.SubmittedAnswers ?? "[]";

            var questions = JsonSerializer.Deserialize<List<QuestionDto>>(questionsJson) ?? new();
            var submittedAnswers = JsonSerializer.Deserialize<List<SubmittedAnswerDto>>(submittedJson) ?? new();

            var detailedAnswers = questions.Select((q, index) =>
            {
                var submitted = submittedAnswers.FirstOrDefault(a => a.QuestionIndex == index);
                return new AnswerReviewDto
                {
                    QuestionIndex = index,
                    Question = q.Question,
                    CorrectAnswer = q.Answer,
                    StudentAnswer = submitted?.Answer ?? "No Answer"
                };
            }).ToList();

            return new ResultDetailDto
            {
                ResultId = result.ResultId,
                AssessmentTitle = result.Assessment?.Title ?? "Unknown",
                StudentName = result.User?.Name ?? "Unknown",
                Score = result.Score,
                AttemptDate = result.AttemptDate,
                Answers = detailedAnswers
            };
        }

        // POST: api/Results/submit
        [Authorize]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitResult([FromBody] QuizSubmissionDto submission)
        {
            if (submission == null || submission.AssessmentId == Guid.Empty)
                return BadRequest("Invalid submission.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var result = new Result
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = submission.AssessmentId,
                UserId = userId,
                AttemptDate = DateTime.UtcNow,
                Score = submission.Score,
                SubmittedAnswers = JsonSerializer.Serialize(submission.Answers)
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/Results/my
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResults()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return BadRequest("Invalid user ID");

            var results = await _context.Results
                .Include(r => r.Assessment)
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.ResultId,
                    r.Score,
                    r.AttemptDate,
                    AssessmentTitle = r.Assessment.Title
                })
                .ToListAsync();

            return Ok(results);
        }

        // DELETE: api/Results/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null)
                return NotFound();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

