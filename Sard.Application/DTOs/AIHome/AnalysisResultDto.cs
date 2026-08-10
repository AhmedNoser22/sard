namespace Sard.Application.DTOs.AIHome
{
    public class AnalysisResultDto
    {
        public string Genre { get; set; } = string.Empty;
        public int Score { get; set; }
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string Verdict { get; set; } = string.Empty;
    }
}
