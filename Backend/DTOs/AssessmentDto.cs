namespace EduSync.Backend.DTOs 
{
    public class AssessmentDto
    {
        public Guid AssessmentId { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public string? Questions { get; set; } // null when not needed
        public string? CourseTitle { get; set; }
    }

    public class CreateAssessmentDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public string Questions { get; set; } = string.Empty;
    }
}