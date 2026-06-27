namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
    {
        public void Configure(EntityTypeBuilder<Chapter> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Chapters)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
