namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class TrimmedBalanceSheetStatement : IHasValue
    {
        public TrimmedBalanceSheetStatement(BalanceSheetStatement x, string reason)
        {
            Value = x.Value;
            Account = x.Account;
            Reason = reason;
        }

        public double Value { get; set; }
        public string Account { get; set; }
        public string Reason { get; set; }
    }
}