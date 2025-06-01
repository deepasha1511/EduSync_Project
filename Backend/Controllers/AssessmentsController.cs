using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EduSync.Backend.Data;
using EduSync.Backend.Models;
using EduSync.Backend.DTOs;

namespace EduSync.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentsController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public AssessmentsController(EduSyncDbContext context)
        {
            _context = context;
        }

        // GET: api/Assessments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessments()
        {
            var assessments = await _context.Assessments
                .Include(a => a.Course)
                .Select(a => new AssessmentDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    MaxScore = a.MaxScore,
                    CourseId = a.CourseId,
                    CourseTitle = a.Course.Title,
                    Questions = a.Questions
                })
                .ToListAsync();

            return Ok(assessments);
        }

        // GET: api/Assessments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentDto>> GetAssessment(Guid id)
        {
            var a = await _context.Assessments.Include(x => x.Course).FirstOrDefaultAsync(x => x.AssessmentId == id);
            if (a == null)
                return NotFound();

            return new AssessmentDto
            {
                AssessmentId = a.AssessmentId,
                CourseId = a.CourseId,
                Title = a.Title,
                MaxScore = a.MaxScore,
                Questions = a.Questions,
                CourseTitle = a.Course?.Title
            };
        }

        // GET: api/Assessments/my
        [Authorize(Roles = "Instructor")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetMyAssessments()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(instructorId, out var instructorGuid))
                return BadRequest("Invalid instructor ID.");

            var assessments = await _context.Assessments
                .Include(a => a.Course)
                .Where(a => a.Course.InstructorId == instructorGuid)
                .Select(a => new AssessmentDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    MaxScore = a.MaxScore,
                    CourseId = a.CourseId,
                    CourseTitle = a.Course.Title,
                    Questions = null
                })
                .ToListAsync();

            return Ok(assessments);
        }

        // POST: api/Assessments
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<ActionResult<AssessmentDto>> PostAssessment(CreateAssessmentDto dto)
        {
            if (!await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId))
                return BadRequest($"Course with ID {dto.CourseId} does not exist");

            if (string.IsNullOrWhiteSpace(dto.Questions))
                return BadRequest("Questions JSON is required");

            var assessment = new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                CourseId = dto.CourseId,
                Title = dto.Title,
                Questions = dto.Questions,
                MaxScore = dto.MaxScore
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssessment), new { id = assessment.AssessmentId }, new AssessmentDto
            {
                AssessmentId = assessment.AssessmentId,
                CourseId = assessment.CourseId,
                Title = assessment.Title,
                MaxScore = assessment.MaxScore,
                Questions = assessment.Questions
            });
        }

        // PUT: api/Assessments/{id}
        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssessment(Guid id, Assessment updated)
        {
            if (id != updated.AssessmentId)
                return BadRequest();

            _context.Entry(updated).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Assessments.Any(e => e.AssessmentId == id))
                    return NotFound();

                throw;
            }
        }

        // DELETE: api/Assessments/{id}
        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null)
                return NotFound();

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
