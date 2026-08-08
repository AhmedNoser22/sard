namespace Sard.Domain.Entities
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public GroupRole Role { get; set; } = GroupRole.Member;
        public DateTime JoinedAt { get; set; } = EgyptDateTime.Now;
    }
}
