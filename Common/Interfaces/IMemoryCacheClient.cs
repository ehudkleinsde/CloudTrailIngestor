namespace Common.Interfaces
{
    public interface IMemoryCacheClient
    {
        object Get(string key);
        void Set(string key, object value, TimeSpan absoluteExpirationRelativeToNow);
    }
}
