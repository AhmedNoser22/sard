namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class HighlightConfiguration : IEntityTypeConfiguration<Highlight>
    {
        public void Configure(EntityTypeBuilder<Highlight> builder)
        {
            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.NovelTitle)
                .HasMaxLength(200);

            builder.Property(x => x.NovelAuthor)
                .HasMaxLength(100);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Highlights)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
