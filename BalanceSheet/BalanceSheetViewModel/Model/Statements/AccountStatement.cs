using System;

namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class AccountStatement : IHasValue
    {
        public int SerialNumber { get; set; }
        public double Value { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double RunningTotaledValue { get; set; }
    }
}