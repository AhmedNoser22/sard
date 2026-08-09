namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class QuoteConfiguration : IEntityTypeConfiguration<Sard.Domain.Entities.Quote>
    {
        public void Configure(EntityTypeBuilder<Sard.Domain.Entities.Quote> builder)
        {
            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Quotes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
