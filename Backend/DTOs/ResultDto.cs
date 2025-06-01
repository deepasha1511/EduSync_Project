namespace EduSync.Backend.DTOs
{
    public class ResultDto
    {
        public Guid ResultId { get; set; }
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;
        public string SubmittedAnswers { get; set; }

        public string UserName { get; set; }
        public string AssessmentTitle { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class SubmitResultDto
    {
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
    }

    public class ResultDetailDto
    {
        public Guid ResultId { get; set; }
        public string AssessmentTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public List<AnswerReviewDto> Answers { get; set; } = new();
    }

    public class AnswerReviewDto
    {
        public int QuestionIndex { get; set; }
        public string Question { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string StudentAnswer { get; set; } = string.Empty;
    }

    public class MyResultDto
    {
        public Guid ResultId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public string AssessmentTitle { get; set; }
    }


}