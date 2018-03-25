using System.ComponentModel;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel
{
    public class DisplayableCorrectedAccountDefintionStatement
    {
        public DisplayableCorrectedAccountDefintionStatement(CorrectedAccountDefintionStatement x)
        {
            AccountType = x.AccountType;
            Account = x.Account;
            RecipientAccount = x.RecipientAccount;
            Reason = x.Reason;
        }

        [DisplayName("Account Tpe")]
        public AccountType AccountType { get; set; }

        public string Account { get; set; }
        public string RecipientAccount { get; set; }
        public string Reason { get; set; }
    }
}