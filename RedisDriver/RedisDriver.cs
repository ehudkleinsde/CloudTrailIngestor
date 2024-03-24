using Common.Interfaces;
using StackExchange.Redis;

namespace Redis
{
    public class RedisDriver : IRedisDriver
    {
        private const string CONN_STR = "redis";
        private const string CLOUD_TRAIL_EVENTS_IDS = "cloudtrail";

        private ConnectionMultiplexer _connection;
        private IDatabase _db;
        private bool _isInit;

        public RedisDriver()
        {
            _isInit = false;
        }

        public async Task InitAsync()
        {
            _connection = await ConnectionMultiplexer.ConnectAsync(CONN_STR);
            _db = _connection.GetDatabase();
        }

        public async Task<bool> AddKeyIfNotExists(string toAdd)
        {
            return await _db.SetAddAsync(CLOUD_TRAIL_EVENTS_IDS, toAdd);
        }
    }
}
