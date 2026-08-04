namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class PostShareConfiguration : IEntityTypeConfiguration<PostShare>
    {
        public void Configure(EntityTypeBuilder<PostShare> builder)
        {
            builder.HasKey(ps => ps.Id);

            builder
                .HasOne(ps => ps.Post)
                .WithMany()
                .HasForeignKey(ps => ps.PostId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
