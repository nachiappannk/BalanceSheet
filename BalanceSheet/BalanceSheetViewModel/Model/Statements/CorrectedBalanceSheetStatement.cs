namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class CorrectedBalanceSheetStatement : IHasValue
    {
        public CorrectedBalanceSheetStatement(BalanceSheetStatement x, string reason)
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