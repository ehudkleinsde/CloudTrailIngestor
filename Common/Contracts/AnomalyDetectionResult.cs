using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Common.Contracts
{
    public class AnomalyDetectionResult
    {
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }
        public CloudTrail CloudTrail { get; set; }
        public int AnomalyScore {  get; set; }
        public string AnomalyDetectionType{ get; set;}
        public string AnomalyDetectionVersion{ get; set;}
        public DateTime ProcessingTimestampUTC { get; set; }
    }
}
