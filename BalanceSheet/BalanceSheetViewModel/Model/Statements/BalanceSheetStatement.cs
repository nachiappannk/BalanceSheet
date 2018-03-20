namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class BalanceSheetStatement : IHasValue
    {
        public string Description { get; set; }
        public double Value { get; set; }
    }
}