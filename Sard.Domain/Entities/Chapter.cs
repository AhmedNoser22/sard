namespace Sard.Domain.Entities
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public DateTime LastEditedAt { get; set; } = EgyptDateTime.Now;

        public int NovelId { get; set; }
        public Novel Novel { get; set; }
    }
}
