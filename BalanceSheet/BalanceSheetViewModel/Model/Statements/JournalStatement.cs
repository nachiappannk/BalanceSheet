using System;

namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class JournalStatement : IHasValue, IHasAccount, ICanClone<JournalStatement>
    {
        public string Account { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public JournalStatement Clone()
        {
            return new JournalStatement()
            {
                Account = Account,
                Date = Date,
                Value = Value,
                Description = Description,
                Tag = Tag,
            };
        }
    }
}