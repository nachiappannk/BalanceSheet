using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public class NominalAccount : IAccount
    {
        private readonly string _accountName;
        private double ledgerValue = 0;

        private readonly List<AccountStatement> _ledgerStatements = new List<AccountStatement>();

        public List<AccountStatement> GetAccountStatements()
        {
            return _ledgerStatements.ToList();
        }

        public List<AccountType> GetPossibleAccountTypes()
        {
            return new List<AccountType>(){AccountType.Notional};
        }

        public List<AccountStatement> GetAccountStatements(AccountType accountType)
        {
            return GetAccountStatements();
        }

        public string GetAccountType()
        {
            return "Nominal Account";
        }

        public NominalAccount(string accountName)
        {
            _accountName = accountName;
        }

        public string GetPrintableName()
        {
            return _accountName.Replace("/","-");
        }

        public void PostStatement(DateTime date, string statement, double value)
        {
            var count = _ledgerStatements.Count + 1;
            _ledgerStatements.Add(new AccountStatement() { Date = date, Description = statement, SerialNumber = count, Value = value });
            ledgerValue += value;
        }

        public double GetAccountValue()
        {
            return ledgerValue;
        }
    }
}