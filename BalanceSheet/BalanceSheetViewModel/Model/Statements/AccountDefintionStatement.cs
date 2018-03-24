namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class AccountDefintionStatement
    {
        public AccountType AccountType { get; set; }
        public string Account { get; set; }

        public string RecipientAccount { get; set; }
    }
}