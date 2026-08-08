namespace Sard.Domain.Entities
{
    public class GroupMessageReaction
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public GroupMessage Message { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public string Emoji { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;
    }
}
