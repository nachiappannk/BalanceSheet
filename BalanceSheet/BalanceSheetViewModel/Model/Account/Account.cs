using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public class Account : IAccount
    {
        private readonly string _accountName;
        private readonly AccountType _accountType;
        private double _ledgerValue;

        private readonly List<AccountStatement> _ledgerStatements = new List<AccountStatement>();

        public Account(string accountName, AccountType accountType)
        {
            _accountName = accountName;
            _accountType = accountType;
            _ledgerValue = 0;
        }
        
        public string GetName()
        {
            return _accountName;
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
            return ledgerStatements.Select(x => new AccountStatement(x)).ToList();
        }

        public AccountType GetAccountType()
        {
            return _accountType;
        }
    }
}