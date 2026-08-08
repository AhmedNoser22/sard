namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class GroupMessageReactionConfiguration : IEntityTypeConfiguration<GroupMessageReaction>
    {
        public void Configure(EntityTypeBuilder<GroupMessageReaction> builder)
        {
            builder.HasIndex(r => new { r.MessageId, r.UserId })
                .IsUnique();

            builder.HasOne(r => r.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
