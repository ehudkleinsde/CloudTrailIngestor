namespace Common.Interfaces
{
    public interface ICassandraDBDriver
    {
        Task InitAsync();
        Task CreateTableAsync();
        Task CreateKeySpaceAsync();
        Task<bool> WriteIfNotExists(string str);
    }
}
