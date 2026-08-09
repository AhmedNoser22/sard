namespace Sard.Infrastructure.Implementation.Cache
{
    public class CacheService(IConnectionMultiplexer? redis, IMemoryCache memoryCache) : ICacheService
    {
        private readonly IDatabase? _db = redis?.GetDatabase();

        public async Task<T?> GetAsync<T>(string key)
        {
            if (_db is not null)
            {
                var value = await _db.StringGetAsync(key);
                if (value.HasValue)
                    return JsonSerializer.Deserialize<T>(value.ToString());
            }
            else
            {
                memoryCache.TryGetValue(key, out T? cached);
                return cached;
            }
            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            if (_db is not null)
                await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(30));
            else
                memoryCache.Set(key, value, expiry ?? TimeSpan.FromMinutes(30));
        }

        public async Task RemoveAsync(string key)
        {
            if (_db is not null)
                await _db.KeyDeleteAsync(key);
            else
                memoryCache.Remove(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            if (_db is not null)
            {
                var server = redis!.GetServer(redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                if (keys.Length > 0)
                    await _db.KeyDeleteAsync(keys);
            }
        }
    }
}