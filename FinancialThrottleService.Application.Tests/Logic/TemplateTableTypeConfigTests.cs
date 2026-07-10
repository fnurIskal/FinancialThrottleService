using FinancialThrottleService.Application.Logic;
using Xunit;

namespace FinancialThrottleService.Application.Tests.Logic
{
    public class TemplateTableTypeConfigTests
    {
        private static readonly int[] MsSourceIds = { 32501 };

        [Fact]
        public void Resolve_RasStaj107_BalanceSheetTemplate21_ReturnsThreeTypes()
        {
            var result = TemplateTableTypeConfig.Resolve("RAS_STAJ107", 21, MsSourceIds);

            Assert.Equal(new[] { 1, 2, 3 }, result);
        }

        [Fact]
        public void Resolve_RasStaj107_Template2_ReturnsQuarterlyAndTtmOnly()
        {
            var result = TemplateTableTypeConfig.Resolve("RAS_STAJ107", 2, MsSourceIds);

            Assert.Equal(new[] { 1, 2 }, result);
        }

        [Fact]
        public void Resolve_RasStaj107_UnknownTemplateId_ReturnsDefaultFourTypes()
        {
            var result = TemplateTableTypeConfig.Resolve("RAS_STAJ107", 9999, MsSourceIds);

            Assert.Equal(new[] { 1, 2, 3, 4 }, result);
        }

        [Fact]
        public void Resolve_RasStaj107_EmptyRuleTemplateId_ReturnsEmptyList()
        {
            var result = TemplateTableTypeConfig.Resolve("RAS_STAJ107", 3, MsSourceIds);

            Assert.Empty(result);
        }

        [Fact]
        public void Resolve_MsSourceDatabase_Template241_ReturnsTwoTypes()
        {
            var result = TemplateTableTypeConfig.Resolve("RAS_STAJ32501", 241, MsSourceIds);

            Assert.Equal(new[] { 1, 2 }, result);
        }

        [Fact]
        public void Resolve_MsSourceDatabase_UnknownTemplateId_ReturnsEmptyList()
        {
            var result = TemplateTableTypeConfig.Resolve("RAS_STAJ32501", 9999, MsSourceIds);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(21, true)]
        [InlineData(281, true)]
        [InlineData(2, false)]
        [InlineData(9999, false)]
        public void IsInflationTemplate_KnownAndUnknownIds_ReturnsCorrectResult(int templateId, bool expected)
        {
            var result = TemplateTableTypeConfig.IsInflationTemplate(templateId);

            Assert.Equal(expected, result);
        }
    }
}
