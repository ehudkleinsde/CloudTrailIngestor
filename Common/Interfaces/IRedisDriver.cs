namespace Common.Interfaces
{
    public interface IRedisDriver
    {
        Task InitAsync();
        Task<bool> AddKeyIfNotExists(string toAdd);
    }
}
