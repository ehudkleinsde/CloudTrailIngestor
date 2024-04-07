using Common.Contracts;
using Common.Interfaces;
using MongoDB.Driver;

namespace MongoDB
{
    public class MongoDBDriver : IDBDriver
    {
        private MongoClient _client;
        private readonly string _dbName;
        private bool _indexInPlace;

        public MongoDBDriver(string connStr, string dbName)
        {
            _client = new MongoClient(connStr);
            _dbName = dbName;
            _indexInPlace = false;
        }

        public async Task<AnomalyDetectionResult> GetAnomalyDetectionResultAsync(AnomalyDetectionResult anomalyDetectionResult)
        {
            await CreateIndexAsync();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<AnomalyDetectionResult>(nameof(AnomalyDetectionResult));
            var idFilter = Builders<AnomalyDetectionResult>.Filter.Eq("CloudTrail.EventId", anomalyDetectionResult.CloudTrail.EventId);
            var typeFilter = Builders<AnomalyDetectionResult>.Filter.Eq("CloudTrail.EventType", anomalyDetectionResult.CloudTrail.EventType.ToString());
            var detectionTypeFilter = Builders<AnomalyDetectionResult>.Filter.Eq("AnomalyDetectionType", anomalyDetectionResult.AnomalyDetectionType);
            return await collection.Find(idFilter & typeFilter & detectionTypeFilter).FirstOrDefaultAsync();
        }

        public async Task UpsertAnomalyDetectionResultAsync(AnomalyDetectionResult anomalyDetectionResult)
        {
            await CreateIndexAsync();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<AnomalyDetectionResult>(nameof(AnomalyDetectionResult));

            var idFilter = Builders<AnomalyDetectionResult>.Filter.Eq("CloudTrail.EventId", anomalyDetectionResult.CloudTrail.EventId);
            var eventTypeFilter = Builders<AnomalyDetectionResult>.Filter.Eq("CloudTrail.EventType", anomalyDetectionResult.CloudTrail.EventType.ToString());
            var detectionTypeFilter = Builders<AnomalyDetectionResult>.Filter.Eq("AnomalyDetectionType", anomalyDetectionResult.AnomalyDetectionType);

            await collection.InsertOneAsync(anomalyDetectionResult);
        }

        private async Task CreateIndexAsync()
        {
            if (_indexInPlace) { return; }

            IMongoCollection<AnomalyDetectionResult> anomalyDetectionCollection = _client.GetDatabase(nameof(AnomalyDetectionResult))
                   .GetCollection<AnomalyDetectionResult>(nameof(AnomalyDetectionResult));

            var anomalyDetectionResultIndex = Builders<AnomalyDetectionResult>.IndexKeys
                .Ascending(x => x.AnomalyDetectionType)
                .Ascending(x => x.CloudTrail.EventId)
                .Ascending(x => x.CloudTrail.EventType);

            await anomalyDetectionCollection.Indexes.CreateOneAsync(new CreateIndexModel<AnomalyDetectionResult>(anomalyDetectionResultIndex));

            _indexInPlace = true;
        }
    }
}
