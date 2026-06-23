using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinancialThrottleService.Domain.Models
{
    [BsonIgnoreExtraElements]
    public class LogEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("Level")]
        public string Level { get; set; } = string.Empty;

        [BsonElement("Category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("Message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("GroupKey")]
        public string? GroupKey { get; set; }

        [BsonElement("SecurityCode")]
        public string? SecurityCode { get; set; }

        [BsonElement("DatabaseName")]
        public string? DatabaseName { get; set; }

        [BsonElement("SecurityId")]
        public int? SecurityId { get; set; }

        [BsonElement("TemplateId")]
        public int? TemplateId { get; set; }

        [BsonElement("Quarter")]
        public int? Quarter { get; set; }

        [BsonElement("Exception")]
        public string? Exception { get; set; }

        [BsonElement("Metadata")]
        public object? Metadata { get; set; }
    }
}