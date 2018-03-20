using System;

namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class LedgerStatement : IHasValue
    {
        public int SerialNumber { get; set; }
        public double Value { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}