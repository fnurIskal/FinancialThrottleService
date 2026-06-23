using FinancialThrottleService.Domain.Models;
using FinancialThrottleService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialThrottleService.Infrastructure.Logging
{
    public class MongoLogRepository : ILogRepository
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoLogRepository> _logger;

        public MongoLogRepository(
            IMongoDatabase database,
            ILogger<MongoLogRepository> logger)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task WriteAsync(LogEntry entry)
        {
            try
            {
                if (entry == null)
                    throw new ArgumentNullException(nameof(entry));

                entry.Timestamp = entry.Timestamp.Kind == DateTimeKind.Utc
                    ? entry.Timestamp
                    : entry.Timestamp.ToUniversalTime();

                // Create collection name based on date: logs_2026_06_23
                var collectionName = $"logs_{entry.Timestamp:yyyy_MM_dd}";
                var collection = _database.GetCollection<LogEntry>(collectionName);

                await collection.InsertOneAsync(entry);

                _logger.LogDebug(
                    "Log entry written to {Collection}: {Category}|{Level}|{Message}",
                    collectionName, entry.Category, entry.Level, entry.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write log entry");
                throw;
            }
        }

        /// <summary>
        /// Gets all logs for a specific date
        /// Queries collection: logs_yyyy_MM_dd
        /// </summary>
        public async Task<LogsResponse> GetLogsByDateAsync(DateTime date)
        {
            try
            {
                var collectionName = $"logs_{date:yyyy_MM_dd}";
                var collection = _database.GetCollection<LogEntry>(collectionName);

                var filter = Builders<LogEntry>.Filter.Empty;

                var logs = await collection
                    .Find(filter)
                    .SortByDescending(x => x.Timestamp)
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} logs from collection {Collection}",
                    logs.Count, collectionName);

                return new LogsResponse
                {
                    Logs = logs,
                    TotalCount = logs.Count,
                    Date = date.Date.ToString("yyyy-MM-dd")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve logs by date: {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// Gets logs by date and category
        /// Queries collection: logs_yyyy_MM_dd with category filter
        /// </summary>
        public async Task<LogsResponse> GetLogsByDateAndCategoryAsync(DateTime date, string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                    throw new ArgumentException("Category cannot be null or empty", nameof(category));

                var collectionName = $"logs_{date:yyyy_MM_dd}";
                var collection = _database.GetCollection<LogEntry>(collectionName);

                var filter = Builders<LogEntry>.Filter.Eq(x => x.Category, category.ToLower());

                var logs = await collection
                    .Find(filter)
                    .SortByDescending(x => x.Timestamp)
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} logs from collection {Collection} with category {Category}",
                    logs.Count, collectionName, category);

                return new LogsResponse
                {
                    Logs = logs,
                    TotalCount = logs.Count,
                    Date = date.Date.ToString("yyyy-MM-dd"),
                    Category = category
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve logs by date and category: {Date}|{Category}",
                    date, category);
                throw;
            }
        }

        /// <summary>
        /// Gets logs by date and level
        /// Queries collection: logs_yyyy_MM_dd with level filter
        /// </summary>
        public async Task<LogsResponse> GetLogsByDateAndLevelAsync(DateTime date, string level)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(level))
                    throw new ArgumentException("Level cannot be null or empty", nameof(level));

                var collectionName = $"logs_{date:yyyy_MM_dd}";
                var collection = _database.GetCollection<LogEntry>(collectionName);

                var filter = Builders<LogEntry>.Filter.Eq(x => x.Level, level);

                var logs = await collection
                    .Find(filter)
                    .SortByDescending(x => x.Timestamp)
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} logs from collection {Collection} with level {Level}",
                    logs.Count, collectionName, level);

                return new LogsResponse
                {
                    Logs = logs,
                    TotalCount = logs.Count,
                    Date = date.Date.ToString("yyyy-MM-dd"),
                    Level = level
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve logs by date and level: {Date}|{Level}",
                    date, level);
                throw;
            }
        }
    }
}