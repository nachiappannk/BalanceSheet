namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class CorrectedBalanceSheetStatement : IHasValue, IHasAccount , ICanClone<CorrectedBalanceSheetStatement>
    {
        public CorrectedBalanceSheetStatement(BalanceSheetStatement x, string reason)
        {
            Value = x.Value;
            Account = x.Account;
            Reason = reason;
        }

        public CorrectedBalanceSheetStatement()
        {
            
        }

        public double Value { get; set; }
        public string Account { get; set; }
        public string Reason { get; set; }
        public CorrectedBalanceSheetStatement Clone()
        {
            return new CorrectedBalanceSheetStatement()
            {
                Value = Value,
                Account = Account,
                Reason = Reason,
            };
        }
    }
}