using StackExchange.Redis;

namespace Services;

public interface ICacheService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan expiry);
    Task RemoveAsync(string key);
}

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _cache;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _cache = redis.GetDatabase();
    }

    public async Task<string?> GetAsync(string key)
    {
        var result = await _cache.StringGetAsync(key);
        return result.HasValue ? result.ToString() : null;
    }

    public Task SetAsync(string key, string value, TimeSpan expiry)
    {
        return _cache.StringSetAsync(key, value, expiry);
    }

    public Task RemoveAsync(string key)
    {
        return _cache.KeyDeleteAsync(key);
    }
}

public class NullCacheService : ICacheService
{
    public Task<string?> GetAsync(string key) => Task.FromResult<string?>(null);
    public Task SetAsync(string key, string value, TimeSpan expiry) => Task.CompletedTask;
    public Task RemoveAsync(string key) => Task.CompletedTask;
}
