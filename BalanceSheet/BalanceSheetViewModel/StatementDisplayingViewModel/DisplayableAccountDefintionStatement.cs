using System.ComponentModel;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel
{
    public class DisplayableAccountDefintionStatement
    {
        public DisplayableAccountDefintionStatement(AccountDefintionStatement x)
        {
            AccountType = x.AccountType;
            Account = x.Account;
            RecipientAccount = x.RecipientAccount;
        }

        [DisplayName("Account Tpe")]
        public AccountType AccountType { get; set; }

        public string Account { get; set; }
        public string RecipientAccount { get; set; }
    }
}