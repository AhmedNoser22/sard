using Sard.Domain.Helpers;

namespace Sard.Domain.Entities
{
    public class PostLike
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;
    }
}
