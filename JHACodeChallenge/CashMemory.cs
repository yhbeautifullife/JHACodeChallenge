using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JHACodeChallenge
{
    public class CashMemory : ICacheMemory
    {
        private IMemoryCache _cache;
        private IConfiguration _config;
        private bool isEnableCache = false;

        public CashMemory(IMemoryCache cache, IConfiguration config)
        {
            _cache = cache;
            _config = config;
            isEnableCache = string.Compare(_config.GetSection("appSettings:EnableCache").Value, "true", true) == 0 ? true : false;

        }
        public void Set<T>(T o, string key)
        {
            if (isEnableCache)
            {
                T cacheEntry = o;

                // set cache options
                var cacheEntryOptions = new MemoryCacheEntryOptions();
                // keep in cache for this period, reset if accessed.
                cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(7200)); // 2h

                // save data in cache
                _cache.Set(key, cacheEntry, cacheEntryOptions);

            }
        }

        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
