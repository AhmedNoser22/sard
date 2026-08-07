namespace Sard.Infrastructure.Jobs
{
    public class TokenCleanupJob(AppDbContext db)
    {
        public async Task CleanExpiredTokensAsync()
        {
            var expired = await db.Set<IdentityUserToken<string>>()
                .Where(t => t.Name == "EmailConfirmation" || t.Name == "ResetPassword")
                .ToListAsync();

            if (expired.Any())
            {
                db.Set<IdentityUserToken<string>>().RemoveRange(expired);
                await db.SaveChangesAsync();
            }
        }
    }
}
