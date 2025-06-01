using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Backend.Models;
using EduSync.Backend.Data;
using EduSync.Backend.DTOs;

namespace EduSync.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public EnrollmentsController(EduSyncDbContext context)
        {
            _context = context;
        }

        // POST: api/Enrollments
        [HttpPost]
        public async Task<ActionResult<object>> CreateEnrollment([FromBody] EnrollmentDto enrollmentDto)
        {
            // Validate course
            var course = await _context.Courses.FindAsync(enrollmentDto.CourseId);
            if (course == null)
                return BadRequest("Course does not exist");

            // Validate user
            var student = await _context.Users.FindAsync(enrollmentDto.UserId);
            if (student == null)
                return BadRequest("User does not exist");

            if (student.Role != "Student")
                return BadRequest("Only students can enroll in courses");

            // Prevent duplicate enrollment
            var exists = await _context.Enrollments
                .AnyAsync(e => e.UserId == enrollmentDto.UserId && e.CourseId == enrollmentDto.CourseId);

            if (exists)
                return Conflict("User is already enrolled in this course");

            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid(),
                CourseId = enrollmentDto.CourseId,
                UserId = enrollmentDto.UserId,
                EnrollmentDate = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.EnrollmentId }, new
            {
                enrollment.EnrollmentId,
                enrollment.UserId,
                UserName = student.Name,
                enrollment.CourseId,
                CourseTitle = course.Title,
                enrollment.EnrollmentDate,
                enrollment.IsCompleted
            });
        }

        // GET: api/Enrollments/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserEnrollments(Guid userId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.CourseId,
                    CourseTitle = e.Course.Title,
                    e.EnrollmentDate,
                    e.IsCompleted
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // GET: api/Enrollments/course/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCourseEnrollments(Guid courseId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Where(e => e.CourseId == courseId)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.UserId,
                    UserName = e.User.Name,
                    e.EnrollmentDate,
                    e.IsCompleted
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // GET: api/Enrollments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEnrollment(Guid id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
                return NotFound();

            return Ok(new
            {
                enrollment.EnrollmentId,
                enrollment.UserId,
                UserName = enrollment.User.Name,
                enrollment.CourseId,
                CourseTitle = enrollment.Course.Title,
                enrollment.EnrollmentDate,
                enrollment.IsCompleted
            });
        }

        // PATCH: api/Enrollments/{id}/complete
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> MarkAsCompleted(Guid id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            enrollment.IsCompleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Enrollments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(Guid id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
