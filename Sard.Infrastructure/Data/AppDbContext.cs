namespace Sard.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Novel> Novels => Set<Novel>();
        public DbSet<Chapter> Chapters => Set<Chapter>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Reply> Replies => Set<Reply>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<Quote> Quotes => Set<Quote>();
        public DbSet<Highlight> Highlights => Set<Highlight>();
        public DbSet<Follow> Follows => Set<Follow>();
        public DbSet<FavoriteNovel> FavoriteNovels => Set<FavoriteNovel>();
        public DbSet<PostShare> PostShares => Set<PostShare>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
