namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class TrialBalanceStatement : IHasValue
    {
        public string Account { get; set; }
        public string Tag { get; set; }
        public double Value { get; set; }
    }
}