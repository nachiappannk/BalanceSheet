using System;

namespace Nachiappan.BalanceSheetViewModel
{
    public class JournalStatement : IHasValue
    {
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string DetailedDescription { get; set; }
        public string Tag { get; set; }
    }


    public class TrimmedJournalStatement : IHasValue
    {
        public TrimmedJournalStatement(JournalStatement x, string reason)
        {
            Description = x.Description;
            Date = x.Date;
            Value = x.Value;
            DetailedDescription = x.DetailedDescription;
            Tag = x.Tag;
            Reason = reason;
        }

        public string Description { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string DetailedDescription { get; set; }
        public string Tag { get; set; }
        public string Reason { get; set; }
    }
}