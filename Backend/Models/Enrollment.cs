using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSync.Backend.Models
{
    public class Enrollment
    {
        public Guid EnrollmentId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CourseId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;

        [Required]
        [ForeignKey("CourseId")]
        public Course Course { get; set; } // Navigation property to Course

        [Required]
        [ForeignKey("UserId")]
        public User User { get; set; } 


    }
}
