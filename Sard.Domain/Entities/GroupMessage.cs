namespace Sard.Domain.Entities
{
    public class GroupMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public string SenderId { get; set; }
        public AppUser Sender { get; set; }

        public ICollection<GroupMessageReaction> Reactions { get; set; } = new List<GroupMessageReaction>();
    }
}