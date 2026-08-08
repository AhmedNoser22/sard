namespace Sard.Domain.Entities
{
    public class Follow
    {
        public string FollowerId { get; set; }
        public AppUser Follower { get; set; }
        public string FollowedId { get; set; }
        public AppUser Followed { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;
    }
}
