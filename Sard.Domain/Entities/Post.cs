using Sard.Domain.Enums;
using Sard.Domain.Helpers;

namespace Sard.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public PostStatus Status { get; set; }
        public int LikesCount { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;
        public DateTime? BannedUntil { get; set; }
        public string? BanReason { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<Reply> Replies { get; set; }
        public ICollection<PostLike> Likes { get; set; }
    }
}
