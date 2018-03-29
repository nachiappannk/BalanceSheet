namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public interface ICanClone<out T>
    {
        T Clone();
    }
}