using Microsoft.Extensions.Caching.Memory;

using System;

namespace ObeTools
{
    public static class CacheHelper
    {
        public static T GetWithCache<T>(this IMemoryCache memoryCache, string key, int ttl, Func<T> func)
        {
            T result = memoryCache.Get<T>(key);
            if (result is null)
            {
                result = func();
                memoryCache.Set(key, result, TimeSpan.FromMinutes(ttl));
            }
            return result;
        }
    }
}
