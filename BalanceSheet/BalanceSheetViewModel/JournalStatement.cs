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
}