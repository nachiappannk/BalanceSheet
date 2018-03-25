using System;

namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class CorrectedJournalStatement : IHasValue
    {
        public CorrectedJournalStatement(JournalStatement x, string reason)
        {
            Account = x.Account;
            Date = x.Date;
            Value = x.Value;
            Description = x.Description;
            Tag = x.Tag;
            Reason = reason;
        }

        public string Account { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public string Reason { get; set; }
    }
}