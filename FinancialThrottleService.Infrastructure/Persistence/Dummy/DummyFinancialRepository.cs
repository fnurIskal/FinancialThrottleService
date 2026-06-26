using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Infrastructure.Persistence.Dummy
{
    public class DummyFinancialRepository : IFinancialRepository
    {
        private readonly List<WaitingGroup> _queue = new()
    {
        new WaitingGroup
        {
            DatabaseName = "RAS_STAJ107",
            SecurityId   = 282,
            TemplateId   = 21,
            SecurityCode = "THYAO",
            OrderType    = 30,
            Items = new List<WaitingItem>
            {
                new() { Quarter = 202409, IsOriginal = false, Username = "boss", DisclosureId = 1001 }
            }
        },

        new WaitingGroup
        {
            DatabaseName = "RAS_STAJ107",
            SecurityId   = 292,
            TemplateId   = 21,
            SecurityCode = "SASA",
            OrderType    = 30,
            Items = new List<WaitingItem>
            {
                new() { Quarter = 202409, IsOriginal = false, Username = "analyst1", DisclosureId = 1002 }
            }
        },

        // MSSTOCK — RAS_STAJ32501, tipler hazır → Koşul 1 geçer
        new WaitingGroup
        {
            DatabaseName = "RAS_STAJ32501",
            SecurityId   = 50,
            TemplateId   = 241,
            SecurityCode = "MSSTOCK",
            OrderType    = 10,
            Items = new List<WaitingItem>
            {
                new() { Quarter = 202412, IsOriginal = false, Username = "analyst2", DisclosureId = 2001 }
            }
        },

        // AKBNK — boss, iki farklı quarter → 202409 gönderilir, 202406 bekler
        new WaitingGroup
        {
            DatabaseName = "RAS_STAJ107",
            SecurityId   = 346,
            TemplateId   = 21,
            SecurityCode = "AKBNK",
            OrderType    = 30,
            Items = new List<WaitingItem>
            {
                new() { Quarter = 202409, IsOriginal = false, Username = "boss", DisclosureId = 1003 },
                new() { Quarter = 202406, IsOriginal = false, Username = "boss", DisclosureId = 1004 }
            }
        }
    };

        
        private readonly Dictionary<string, List<int>> _tableTypeIds = new()
    {
        { "RAS_STAJ107|282|21|202409",   new List<int> { 1, 2, 3 } },  
        { "RAS_STAJ107|292|21|202409",   new List<int> { 1, 2 } },     
        { "RAS_STAJ32501|50|241|202412", new List<int> { 1, 2 } },     
        { "RAS_STAJ107|346|21|202409",   new List<int> { 1, 2, 3 } },  
        { "RAS_STAJ107|346|21|202406",   new List<int> { 1 } },
    };


        private readonly HashSet<string> _quarterlyData = new()
    {
        "RAS_STAJ107|292|21|202409"
    };


        public Task<List<WaitingGroup>> GetAllWaitingGroupsAsync()
        {
            return Task.FromResult(new List<WaitingGroup>(_queue));
        }

        public Task<List<int>> GetTableTypeIdsAsync(
            string databaseName, int securityId,
            int quarter, int templateId, bool isOriginal)
        {
            var key = $"{databaseName}|{securityId}|{templateId}|{quarter}";
            var result = _tableTypeIds.TryGetValue(key, out var ids)
                ? ids
                : new List<int>();

            return Task.FromResult(result);
        }

        public Task<int> GetDisclosureQuarterAsync(
            int securityId, int disclosureId,
            string databaseName, int templateId)
        {
            return Task.FromResult(1);
        }

        public Task ExecuteSendAsync(
            string databaseName, int securityId, int quarter,
            int templateId, int disclosureId, bool isOriginal,
            List<int> tableTypeIds, bool isInflationTemplate, bool sendEmail)
        {
            Console.WriteLine(
                $"[DUMMY] ExecuteSend → {databaseName}|{securityId}|{templateId} " +
                $"Quarter={quarter} IsOriginal={isOriginal} " +
                $"TypeIds=[{string.Join(",", tableTypeIds)}] " +
                $"Inflation={isInflationTemplate} SendEmail={sendEmail}");

            var group = _queue.FirstOrDefault(g =>
                g.DatabaseName == databaseName &&
                g.SecurityId == securityId &&
                g.TemplateId == templateId);

            group?.Items.RemoveAll(i =>
                i.Quarter == quarter &&
                i.IsOriginal == isOriginal);

            return Task.CompletedTask;
        }

        public Task WriteHeartbeatAsync()
        {
            Console.WriteLine($"[DUMMY] Heartbeat → {DateTime.UtcNow:HH:mm:ss}");
            return Task.CompletedTask;
        }

        public Task WriteAliveSqlAsync()
        {
            Console.WriteLine($"[DUMMY] AliveSql → {DateTime.UtcNow:HH:mm:ss}");
            return Task.CompletedTask;
        }

        public Task<Dictionary<int, string>> GetSecurityCodesAsync(int[] securityIds)
        {
            var map = new Dictionary<int, string>
        {
            { 282, "THYAO" },
            { 292, "SASA"  },
            { 346, "AKBNK" },
            { 50,  "MSSTOCK" }
        };

            var result = securityIds
                .Where(map.ContainsKey)
                .ToDictionary(id => id, id => map[id]);

            return Task.FromResult(result);
        }

        public Task<Dictionary<(int securityId, int templateId), int>> GetDuplicateItemCodeMapAsync()
        {
            var map = new Dictionary<(int, int), int>
        {
            { (282, 21), 1 }
        };

            return Task.FromResult(map);
        }

        public Task<int[]> GetMsSourceIdsAsync()
        {
            return Task.FromResult(new[] { 32501 });
        }

        public Task<bool> HasQuarterlyDataAsync(
            string databaseName, int securityId,
            int quarter, int templateId, bool isOriginal)
        {
            var key = $"{databaseName}|{securityId}|{templateId}|{quarter}";
            return Task.FromResult(_quarterlyData.Contains(key));
        }

        public Task DeleteFromQueueAsync(
            string databaseName, int securityId, int quarter, int templateId,
            int disclosureId, bool isOriginal, List<int> tableTypeIds)
        {
            Console.WriteLine($"[DUMMY] DeleteFromQueueAsync: {databaseName}/{securityId}/Q{quarter}/T{templateId}");
            return Task.CompletedTask;
        }
    }
}