using FinancialThrottleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Interfaces
{
    public interface IFinancialRepository
    {
        Task<List<WaitingGroup>> GetAllWaitingGroupsAsync();
        Task<List<int>> GetTableTypeIdsAsync(
       string databaseName,
       int securityId,
       int quarter,
       int templateId,
       bool isOriginal);
        Task<int> GetDisclosureQuarterAsync(
       int securityId,
       int disclosureId,
       string databaseName,
       int templateId);
        Task ExecuteSendAsync(
    string databaseName,
    int securityId,
    int quarter,
    int templateId,
    int disclosureId,
    bool isOriginal,
    List<int> tableTypeIds,
    bool isInflationTemplate,
    bool sendEmail);
        Task WriteHeartbeatAsync();
        Task WriteAliveSqlAsync();
        Task<Dictionary<int, string>> GetSecurityCodesAsync(int[] securityIds);
        Task<Dictionary<(int securityId, int templateId), int>> GetDuplicateItemCodeMapAsync();
        Task<int[]> GetMsSourceIdsAsync();
        Task<bool> HasQuarterlyDataAsync(
        string databaseName,
        int securityId,
        int quarter,
        int templateId,
        bool isOriginal);
        Task DeleteFromQueueAsync(
        string databaseName,
        int securityId,
        int quarter,
        int templateId,
        int disclosureId,
        bool isOriginal,
        List<int> tableTypeIds,
        bool forceSend = false);
    }
}
