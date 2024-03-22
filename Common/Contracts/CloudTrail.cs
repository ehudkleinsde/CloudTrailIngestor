using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Common.Contracts
{
    public class CloudTrail
    {
        [BsonIgnoreIfDefault]
        [JsonIgnore]
        public ObjectId _id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid RequestId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid EventId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid RoleId { get; set; }
        public DateTime TimestampUTC { get; set; }
        public string[] AffectedAssets { get; set; }

        [BsonRepresentation(BsonType.String)]
        public EventType EventType { get; set; }
    }

    public enum EventType
    {
        Create,
        Read,
        Update, 
        Delete
    }
}
