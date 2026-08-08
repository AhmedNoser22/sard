namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
    {
        public void Configure(EntityTypeBuilder<GroupMember> builder)
        {
            builder.HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();

            builder.Property(x => x.Role).HasConversion<string>();

            builder.HasOne(x => x.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
