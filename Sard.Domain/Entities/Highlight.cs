using Sard.Domain.Helpers;

namespace Sard.Domain.Entities
{
    public class Highlight
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string? NovelTitle { get; set; }
        public string? NovelAuthor { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
