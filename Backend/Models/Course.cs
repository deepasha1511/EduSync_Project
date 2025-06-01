using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.Backend.Models
{
    public class Course
    {
        [Key]
        public Guid CourseId { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 

        [Required]
        public Guid InstructorId { get; set; }  // Instructor's UserId
        public string MediaUrl { get; set; } = string.Empty;    
        public User? Instructor { get; set; }  // Navigation property
        public ICollection<Assessment>? Assessments { get; set; } = new List<Assessment>();
        public ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();

    }
}
