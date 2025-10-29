using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class CacheService(IMemoryCache cache)
{
    private readonly IMemoryCache _cache = cache;
    private readonly HashSet<string> _keys = [];

    public T? GetOrCreate<T>(string key, Func<ICacheEntry, T> factory)
    {
        var value = _cache.GetOrCreate(key, entry =>
            {
                var result = factory(entry);
                _keys.Add(key);
                entry.RegisterPostEvictionCallback((_, _, _, _) => _keys.Remove(key));
                return result;
            });

        return value!;
    }
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);

        _cache.Set(key, value, options);
        _keys.Add(key);

        // When the item expires, remove the key
        options.RegisterPostEvictionCallback((_, _, _, _) => _keys.Remove(key));
    }

    public bool TryGet<T>(string key, out T value)
    {
        return _cache.TryGetValue(key, out value!);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.Remove(key);
    }

    public object? Get(string key)
    {
        return _cache.Get(key);
    }

    public IEnumerable<string> GetAllKeys() => _keys;
}