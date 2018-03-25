namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class CorrectedAccountDefintionStatement
    {
        public CorrectedAccountDefintionStatement(AccountDefintionStatement statement, string reason)
        {
            AccountType = statement.AccountType;
            Account = statement.Account;
            RecipientAccount = statement.RecipientAccount;
            Reason = reason;
        }

        public AccountType AccountType { get; set; }
        public string Account { get; set; }
        public string RecipientAccount { get; set; }
        public string Reason { get; set; }
    }
}