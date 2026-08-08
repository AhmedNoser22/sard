namespace Sard.Domain.Entities
{
    public class FavoriteNovel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? AuthorName { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
