namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
    {
        public void Configure(EntityTypeBuilder<Quote> builder)
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
