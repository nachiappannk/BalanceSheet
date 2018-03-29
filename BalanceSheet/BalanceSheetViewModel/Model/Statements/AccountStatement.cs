using System;

namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class AccountStatement : IHasValue
    {
        public AccountStatement(AccountStatement x)
        {
            SerialNumber = x.SerialNumber;
            Date = x.Date;
            Description = x.Description;
            RunningTotaledValue = x.RunningTotaledValue;
            Value = x.Value;
        }

        public AccountStatement()
        {
        }

        public int SerialNumber { get; set; }
        public double Value { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double RunningTotaledValue { get; set; }
    }
}