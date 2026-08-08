namespace Sard.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool AgreeToTerms { get; set; }
        public DateTime CreatedAt { get; set; } = EgyptDateTime.Now;

        public ICollection<Novel> Novels { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Quote> Quotes { get; set; }
        public ICollection<Highlight> Highlights { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
        public ICollection<Follow> Followers { get; set; }
        public ICollection<Follow> Following { get; set; }
        public ICollection<FavoriteNovel> FavoriteNovels { get; set; }
        public ICollection<GroupMember> GroupMemberships { get; set; }
    }
}
