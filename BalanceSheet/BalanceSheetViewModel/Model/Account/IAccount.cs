using System;
using System.Collections.Generic;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public interface IAccount
    {
        string GetPrintableName();
        void PostStatement(DateTime date, string statement, double value);
        double GetAccountValue();
        List<AccountStatement> GetAccountStatements();
        List<AccountType> GetPossibleAccountTypes();
        List<AccountStatement> GetAccountStatements(AccountType accountType);
    }
}