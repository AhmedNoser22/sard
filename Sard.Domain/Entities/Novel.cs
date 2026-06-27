using Sard.Domain.Enums;
using Sard.Domain.Helpers;

namespace Sard.Domain.Entities
{
    public class Novel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal Price { get; set; }
        public NovelStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;

        public string AuthorId { get; set; }
        public AppUser Author { get; set; }

        public ICollection<Chapter> Chapters { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
    }
}
