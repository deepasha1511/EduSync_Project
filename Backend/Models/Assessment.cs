using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.Backend.Models
{
    public class Assessment
    {
        [Key]
        public Guid AssessmentId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CourseId { get; set; }  // Foreign key to Course

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Questions { get; set; } = string.Empty; // JSON or other format for questions

        [Required]
        public int MaxScore { get; set; }

        public Course? Course { get; set; }  // Navigation property
        public ICollection<Result>? Results { get; set; } = new List<Result>();
    }
}
