namespace EduSync.Backend.DTOs;

public class CourseDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public string? MediaUrl { get; set; }

    public List<AssessmentDto>? Assessments { get; set; }  // Optional for detailed course view
}

public class CreateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public Guid InstructorId { get; set; }
}

public class CourseUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
}