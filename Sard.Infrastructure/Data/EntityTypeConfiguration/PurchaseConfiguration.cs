namespace Sard.Infrastructure.Data.EntityTypeConfiguration
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Type)
                .HasConversion<string>();

            builder.Property(x => x.PaymobTransactionId)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Purchases)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Purchases)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
