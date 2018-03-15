using System;
using System.Collections.Generic;
using System.Linq;

namespace Nachiappan.BalanceSheetViewModel
{
    public interface IHasValue
    {
        double Value { get; set; }
    }

    public static class HasValueExtentions
    {

        public static double GetCreditValue(this IHasValue hasValue)
        {
            return hasValue.Value > 0 ? hasValue.Value : 0;
        }

        public static double GetDebitValue(this IHasValue hasValue)
        {
            return hasValue.Value <= 0 ? -1 * hasValue.Value : 0;
        }

        public static double GetTotal(this IEnumerable<IHasValue> statements)
        {
            var total = statements.Sum(s => s.Value);
            if (IsZero(total)) return 0;
            return total;
        }

        public static double GetCreditTotal(this IEnumerable<IHasValue> statements)
        {
            return statements.Where(x => x.Value > 0).GetTotal();
        }

        public static double GetDebitTotal(this IEnumerable<IHasValue> statements)
        {
            return -1 * statements.Where(x => x.Value <= 0).GetTotal();
        }

        private static bool IsZero(double value)
        {
            return Math.Abs(value) < 0.001;
        }
    }
}