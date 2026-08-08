namespace Sard.Domain.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;

        public string CreatorId { get; set; }
        public AppUser Creator { get; set; }

        public ICollection<GroupMember> Members { get; set; }
        public ICollection<GroupMessage> Messages { get; set; }
    }
}
