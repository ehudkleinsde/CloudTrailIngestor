namespace Cache
{
    using Common.Interfaces;
    using Microsoft.Extensions.Caching.Memory;

    public class MemoryCacheClient : IMemoryCacheClient
    {
        private IMemoryCache cache;

        public MemoryCacheClient()
        {
            //TODO: set size limit
            this.cache = new MemoryCache(new MemoryCacheOptions());
        }

        public object Get(string key)
        {
            return cache.Get(key);
        }

        public void Set(string key, object value, TimeSpan absoluteExpirationRelativeToNow)
        {
            cache.Set(key, value, absoluteExpirationRelativeToNow);
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }
    }
}
