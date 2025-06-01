using Microsoft.AspNetCore.Mvc;
using EduSync.Backend.Data;
using EduSync.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using EduSync.Backend.Services;
using EduSync.Backend.DTOs;

namespace EduSync.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        //private readonly BlobService _blobService;
        private readonly EduSyncDbContext _context;

        public CoursesController(EduSyncDbContext context)
        {
           // _blobService = blobService;
            _context = context;
        }


        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    MediaUrl = c.MediaUrl
                })
                .ToListAsync();
        }

        // GET: api/Courses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourseById(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Assessments)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            return new CourseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl,
                Assessments = course.Assessments?.Select(a => new AssessmentDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    MaxScore = a.MaxScore
                }).ToList()
            };
        }

        // GET: api/Courses/my
        [Authorize(Roles = "Instructor")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetMyCourses()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(instructorId, out var instructorGuid))
                return BadRequest("Invalid instructor ID.");

            var courses = await _context.Courses
                .Where(c => c.InstructorId == instructorGuid)
                .Include(c => c.Instructor)
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    MediaUrl = c.MediaUrl
                })
                .ToListAsync();

            return Ok(courses);
        }

        // POST: api/Courses
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<ActionResult<CourseDto>> PostCourse(CreateCourseDto dto)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(instructorId, out var instructorGuid))
                return BadRequest("Invalid instructor ID.");

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                InstructorId = instructorGuid,
                MediaUrl = dto.MediaUrl
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourseById), new { id = course.CourseId }, new CourseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            });
        }

        // PUT: api/Courses/{id}
        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(Guid id, CourseUpdateDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.MediaUrl = dto.MediaUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Courses/{id}
        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Courses/enroll/{courseId}
        [Authorize(Roles = "Student")]
        [HttpPost("enroll/{courseId}")]
        public async Task<IActionResult> Enroll(Guid courseId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID.");

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (alreadyEnrolled)
                return Conflict("Already enrolled.");

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                UserId = userId,
                EnrollmentDate = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrollment successful." });
        }
    }
}
