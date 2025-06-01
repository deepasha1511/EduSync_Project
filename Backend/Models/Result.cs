using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.Backend.Models
{
    public class Result
    {
        [Key]
        public Guid ResultId { get; set; } = Guid.NewGuid();  // Unique identifier for the result

        [Required]
        public Guid AssessmentId { get; set; }  // Foreign key to Assessment
        public Assessment? Assessment { get; set; }  // Navigation property
        public string SubmittedAnswers { get; set; } = string.Empty;  // JSON or other format for submitted answers

        [Required]
        public Guid UserId { get; set; }  // Foreign key to User
        public User? User { get; set; }  // Navigation property

        [Required]
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;  // Date and time of the attempt
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    }
}
