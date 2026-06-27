using Sard.Domain.Helpers;

namespace Sard.Domain.Entities
{
    public class Quote
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
