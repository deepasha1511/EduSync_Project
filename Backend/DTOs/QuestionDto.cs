namespace EduSync.Backend.DTOs
{
    // Helper classes for JSON parsing
    public class QuestionDto
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new();
        public string Answer { get; set; } = "";
    }
}