using FinancialThrottleService.Domain.Models;
using FinancialThrottleService.Application.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FinancialThrottleService.Infrastructure.Logging
{
    public class MongoLogRepository : ILogRepository
    {
        private readonly IMongoDatabase _database;
        private readonly MongoOptions _options;

        public MongoLogRepository(IOptions<MongoOptions> options)
        {
            _options = options.Value;
            var client = new MongoClient(_options.ConnectionString);
            _database = client.GetDatabase(_options.DatabaseName);
        }

        // YAZ
        public async Task WriteAsync(LogEntry entry)
        {
            entry.Timestamp = DateTime.UtcNow;
            var collectionName = $"logs_{entry.Timestamp:yyyy_MM_dd}";
            var collection = _database.GetCollection<LogEntry>(collectionName);
            await collection.InsertOneAsync(entry);
        }

        // OKU
        public async Task<List<LogEntry>> QueryAsync(
            string date,
            string? category = null,
            string? level = null,
            int skip = 0,
            int take = 100)
        {
            var collection = _database.GetCollection<LogEntry>($"logs_{date}");
            var builder = Builders<LogEntry>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrEmpty(category))
                filter &= builder.Eq(l => l.Category, category);
            if (!string.IsNullOrEmpty(level))
                filter &= builder.Eq(l => l.Level, level);

            return await collection.Find(filter)
                .Sort(Builders<LogEntry>.Sort.Descending(l => l.Timestamp))
                .Skip(skip).Limit(take)
                .ToListAsync();
        }

        // STREAM (BOŞTA)
        public IAsyncEnumerable<LogEntry> StreamAsync(
            string date,
            string? category,
            CancellationToken ct)
        {
            return new EmptyAsyncEnumerable();
        }

        private class EmptyAsyncEnumerable : IAsyncEnumerable<LogEntry>
        {
            public IAsyncEnumerator<LogEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new EmptyAsyncEnumerator();
            }
        }

        private class EmptyAsyncEnumerator : IAsyncEnumerator<LogEntry>
        {
            public LogEntry Current => throw new NotImplementedException();
            public ValueTask DisposeAsync() => default;
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(false);
        }
    }

    public class MongoOptions
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "financial_throttle_logs";
        public int RetentionDays { get; set; } = 30;
    }
}