using Common.Interfaces;


namespace Cassandra
{
    public class CassandraDBDriver : ICassandraDBDriver
    {
        private const string KEY_SPACE = "CloudTrailIngestor";
        private const string TABLE = "CloudTrailEventsIds";
        private const string KEY = "CloudTrailEventsId";
        private const string POINT_OF_CONTACT = "cassandra";
        
        private PreparedStatement _preparedStatement;

        private Cluster _cluster;
        private ISession _session;

        private bool _isInit;

        public CassandraDBDriver()
        {
            _isInit = false;
        }

        public async Task InitAsync()
        {
            if(!_isInit)
            {
                _cluster = Cluster.Builder().AddContactPoint(POINT_OF_CONTACT).Build();
                _session = await _cluster.ConnectAsync();

                await CreateKeySpaceAsync();
                await CreateTableAsync();
                await PrepareStatementAsync();
                _isInit = true;
            }
        }

        public async Task PrepareStatementAsync()
        {
            _preparedStatement = _session.Prepare($"INSERT INTO {KEY_SPACE}.{TABLE} ({KEY}) VALUES (?) IF NOT EXISTS;");
            _preparedStatement.SetConsistencyLevel(ConsistencyLevel.One);

        }

        public async Task CreateTableAsync()
        {
            _session.Execute($"CREATE TABLE IF NOT EXISTS {KEY_SPACE}.{TABLE} ({KEY} text PRIMARY KEY)");
        }

        public async Task CreateKeySpaceAsync()
        {
            _session.Execute("CREATE KEYSPACE IF NOT EXISTS "+KEY_SPACE+" WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 }");
            _session.Execute($"USE {KEY_SPACE}");
        }

        /// <summary>
        /// Write unless exists
        /// </summary>
        /// <param name="str">string to write</param>
        /// <returns>true if written, so true if didn't exist earlier</returns>
        public async Task<bool> WriteIfNotExists(string str)
        {
            var boundStatement = _preparedStatement.Bind(str);

            var resultSet = await _session.ExecuteAsync(boundStatement);
            var row = resultSet.FirstOrDefault();

            if (row != null)
            {
                return row.GetValue<bool>("[applied]");
            }

            return false;
        }

    }
}
