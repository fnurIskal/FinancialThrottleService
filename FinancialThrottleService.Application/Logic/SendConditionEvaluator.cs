using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Logic
{
    public class SendConditionEvaluator
    {
        private readonly IFinancialRepository _repository;
        private readonly string _turkeyDb;

        public SendConditionEvaluator(
            IFinancialRepository repository,
            string turkeyDb)
        {
            _repository = repository;
            _turkeyDb = turkeyDb;
        }

        public async Task<SendCondition> EvaluateAsync(
            WaitingGroup group,
            WaitingItem item,
            List<int> tableTypeIds,
            IReadOnlyList<int> waitingTableTypeIds)
        {
            if (tableTypeIds.Count == 0)
                return SendCondition.Skip;

            bool allPresent = waitingTableTypeIds.All(t => tableTypeIds.Contains(t));
            if (allPresent)
                return SendCondition.Send;

            bool isBossAndTurkeyDb =
                item.Username == "boss" &&
                group.DatabaseName == _turkeyDb;

            if (!isBossAndTurkeyDb)
            {
                bool hasQuarterly = await _repository.HasQuarterlyDataAsync(
                    group.DatabaseName,
                    group.SecurityId,
                    item.Quarter,
                    group.TemplateId,
                    item.IsOriginal);

                return hasQuarterly ? SendCondition.Send : SendCondition.Wait;
            }

            int disclosureQuarter = await _repository.GetDisclosureQuarterAsync(
                group.SecurityId,
                item.DisclosureId,
                group.DatabaseName,
                group.TemplateId);

            if (disclosureQuarter <= 1 || item.Quarter == disclosureQuarter)
                return SendCondition.Wait;

            bool hasCurrentQuarter = await _repository.HasQuarterlyDataAsync(
                group.DatabaseName,
                group.SecurityId,
                item.Quarter,
                group.TemplateId,
                item.IsOriginal);

            bool hasDisclosureQuarter = await _repository.HasQuarterlyDataAsync(
                group.DatabaseName,
                group.SecurityId,
                disclosureQuarter,
                group.TemplateId,
                item.IsOriginal);

            return (hasCurrentQuarter && hasDisclosureQuarter)
                ? SendCondition.Send
                : SendCondition.Wait;
        }
    }
}
