using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Helper
{
    /// <summary>
    /// Extension method for calculating standard deviation
    /// </summary>
    public static class EnumerableExtensions
    {
        public static decimal StandardDeviation(this IEnumerable<decimal> values)
        {
            var valueList = values.ToList();
            if (!valueList.Any()) return 0m;

            var average = valueList.Average();
            var sumOfSquaresOfDifferences = valueList.Select(val => (val - average) * (val - average)).Sum();
            return (decimal)Math.Sqrt((double)(sumOfSquaresOfDifferences / valueList.Count));
        }
    }

}
