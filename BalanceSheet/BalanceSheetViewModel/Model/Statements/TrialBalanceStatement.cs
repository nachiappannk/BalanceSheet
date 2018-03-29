namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class TrialBalanceStatement : IHasValue, IHasAccount, ICanClone<TrialBalanceStatement>
    {
        public string Account { get; set; }
        public string Tag { get; set; }
        public double Value { get; set; }
        public TrialBalanceStatement Clone()
        {
            return new TrialBalanceStatement()
            {
                Account = Account,
                Tag = Tag,
                Value = Value,
            };
        }
    }
}