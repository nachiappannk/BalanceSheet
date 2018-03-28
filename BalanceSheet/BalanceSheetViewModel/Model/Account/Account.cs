using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public class Account : IAccount
    {
        private readonly string _accountName;
        private readonly string _printableAccountName;
        private readonly AccountType _accountType;
        private double _ledgerValue;

        private readonly List<AccountStatement> _ledgerStatements = new List<AccountStatement>();

        public Account(string accountName, AccountType accountType, Dictionary<string,string> accountNamesLookUp)
        {
            _accountName = accountName;
            _printableAccountName = accountName;
            if (accountNamesLookUp.ContainsKey(_accountName)) _printableAccountName = accountNamesLookUp[_accountName];
            _accountType = accountType;
            _ledgerValue = 0;
        }
        
        public string GetName()
        {
            return _accountName;
        }

        public string GetPrintableName()
        {
            return _printableAccountName;
        }


        public void PostStatement(DateTime date, string statement, double value)
        {
            var count = _ledgerStatements.Count + 1;
            _ledgerValue += value;
            _ledgerStatements.Add(new AccountStatement()
            {
                Date = date,
                Description = statement,
                SerialNumber = count,
                Value = value,
                RunningTotaledValue = _ledgerValue,
            });
            
        }

        public double GetAccountValue()
        {
            return _ledgerValue;
        }

        public List<AccountStatement> GetRawTypeIndependentAccountStatements()
        {
            return _ledgerStatements.ToList();
        }

        public List<AccountStatement> GetAccountStatements()
        {
            var ledgerStatements = GetRawTypeIndependentAccountStatements();


            var accountStatements = ledgerStatements.Select(x => new AccountStatement(x)).ToList();
            if (_accountType == AccountType.Asset) InvertStatementValues(accountStatements);
            if (_accountType == AccountType.Liability) InvertStatementValues(accountStatements);



            return accountStatements;
        }

        private static void InvertStatementValues(List<AccountStatement> accountStatements)
        {
            accountStatements.ForEach(x => x.Value = x.Value * -1);
            accountStatements.ForEach(x => x.RunningTotaledValue = x.RunningTotaledValue * -1);
        }

        public AccountType GetAccountType()
        {
            return _accountType;
        }
    }
}