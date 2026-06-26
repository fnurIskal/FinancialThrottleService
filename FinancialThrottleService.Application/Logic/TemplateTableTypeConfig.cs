using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Logic
{
    public static class TemplateTableTypeConfig
    {
        public static IReadOnlyList<int> Resolve(
           string databaseName,
           int templateId,
           int[] msSourceIds)
        {
            if (msSourceIds.Any(id => databaseName.Contains(id.ToString())))
            {
                if (new[] { 241, 243, 251, 252, 267, 268, 291 }.Contains(templateId))
                    return new[] { 1, 2 };

                if (new[] { 242, 247, 248, 249, 250, 265, 266, 289 }.Contains(templateId))
                    return new[] { 1, 2, 3 };

                return Array.Empty<int>();
            }

                if (databaseName is "RAS_STAJ107")
            {
                if (new[] { 2, 5, 8, 11, 14, 31, 36 }.Contains(templateId))
                    return new[] { 1, 2 };

                if (templateId == 65)
                    return new[] { 1, 2, 3 };

                if (new[] { 3, 4, 6, 7, 9, 10, 12, 13, 32, 33, 256, 257, 192, 193 }.Contains(templateId))
                    return Array.Empty<int>();

                if (new[] { 21, 281, 282 }.Contains(templateId))
                    return new[] { 1, 2, 3 };

               
                return new[] { 1, 2, 3, 4 };
            }

            if (databaseName == "RAS_601")
            {
                if (new[] { 60, 61 }.Contains(templateId))
                    return new[] { 1, 2, 3 };

                if (new[] { 59, 182, 195, 196, 214, 215, 216, 228, 229 }.Contains(templateId))
                    return new[] { 1, 2 };

                return Array.Empty<int>();
            }

           
            if (databaseName == "RAS_651")
            {
                if (new[] { 220, 221, 224, 225 }.Contains(templateId))
                    return new[] { 1, 2, 3 };

                if (new[] { 222, 223, 230 }.Contains(templateId))
                    return new[] { 1, 2 };

                return Array.Empty<int>();
            }

            if (databaseName == "RAS_801")
            {
                if (new[] { 244, 245 }.Contains(templateId))
                    return new[] { 1, 2, 3 };

                if (new[] { 253, 254, 90 }.Contains(templateId))
                    return new[] { 1, 2 };

                return Array.Empty<int>();
            }

            if (databaseName == "RAS_501")
            {
                if (new[] { 226, 227, 84, 202 }.Contains(templateId))
                    return new[] { 1, 2, 3 };

                if (new[] { 74, 85, 86, 87, 88, 89, 203 }.Contains(templateId))
                    return new[] { 1, 2 };

                return Array.Empty<int>();
            }

            if (databaseName is "RAS_STAJ32501" or "RAS_32501")
            {
                if (new[] { 241, 243, 251, 252, 267, 268 }.Contains(templateId))
                    return new[] { 1, 2 };

                if (new[] { 242, 247, 248, 249, 250, 265, 266 }.Contains(templateId))
                    return new[] { 1, 2, 3 };

                return Array.Empty<int>();
            }

            return Array.Empty<int>();
        }

       
        public static bool IsInflationTemplate(int templateId)
        {
            return new[]
            {
            1, 15, 16, 17, 18, 19, 21, 23, 31, 32, 33,
            98, 192, 193, 194, 255, 256, 257, 281, 282
        }.Contains(templateId);
        }
    }
}
