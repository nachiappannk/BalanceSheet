namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class AccountDefintionStatement : IHasAccount, IHasRecipientAccount, ICanClone<AccountDefintionStatement>
    {
        public AccountType AccountType { get; set; }
        public string Account { get; set; }

        public string RecipientAccount { get; set; }
        public AccountDefintionStatement Clone()
        {
            var statement = new AccountDefintionStatement
            {
                AccountType = AccountType,
                Account = Account,
                RecipientAccount = RecipientAccount ?? string.Empty
            };
            return statement;
        }
    }
}