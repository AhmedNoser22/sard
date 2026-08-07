using Sard.Application.Interfaces.Cache;

namespace Sard.Infrastructure.Jobs
{
    public class StatsUpdateJob(AppDbContext db, ICacheService cache)
    {
        public async Task UpdateNovelStatsAsync()
        {
            var novels = await db.Novels
                .Where(n => n.Status == NovelStatus.Published)
                .ToListAsync();

            foreach (var novel in novels)
            {
                var readCount = await db.Purchases
                    .CountAsync(p => p.NovelId == novel.Id && p.Type == PurchaseType.ReadFee);
                novel.ReadCount = readCount;
            }

            await db.SaveChangesAsync();

            await cache.RemoveByPrefixAsync("posts:");
            await cache.RemoveByPrefixAsync("profile:");
        }
    }
}
