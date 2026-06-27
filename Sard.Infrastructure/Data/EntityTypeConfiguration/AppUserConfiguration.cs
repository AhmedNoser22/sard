namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Bio)
                .HasMaxLength(500);

            builder.Property(x => x.ProfileImageUrl)
                .HasMaxLength(500);
        }
    }
}
