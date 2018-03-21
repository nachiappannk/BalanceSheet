using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public class RealAccount : IAccount
    {
        private readonly string _accountName;
        private double ledgerValue = 0;

        private List<AccountStatement> ledgerStatements = new List<AccountStatement>();

        public RealAccount(string accountName, DateTime openingDate, double value)
        {
            _accountName = accountName;
            ledgerStatements.Add(new AccountStatement()
            {
                Date = openingDate,
                Description = "Opening",
                SerialNumber = 1,
                Value =  value
            });
            ledgerValue = value;

        }

        public string GetPrintableName()
        {
            return _accountName;
        }

       
        public void PostStatement(DateTime date, string statement, double value)
        {
            var count = ledgerStatements.Count + 1;
            ledgerStatements.Add(new AccountStatement() { Date = date, Description = statement, SerialNumber = count, Value = value });
            ledgerValue += value;
        }

        public double GetAccountValue()
        {
            return ledgerValue;
        }

        public List<AccountStatement> GetAccountStatements()
        {
            return ledgerStatements.ToList();
        }

        public List<AccountStatement> GetAccountStatements(AccountType accountType)
        {
            bool isInversionRequired = false;
            if (GetPossibleAccountTypes().Contains(accountType))
            {
                if (accountType == AccountType.Asset) isInversionRequired = true;
                if (accountType == AccountType.Liability) isInversionRequired = true;
                if (accountType == AccountType.Equity) isInversionRequired = false;
            }

            var ledgerStatements = GetAccountStatements();

            double factor = 1;
            if(isInversionRequired) factor = -1;
            return ledgerStatements.Select(x => new AccountStatement()
            {
                SerialNumber = x.SerialNumber,
                Date = x.Date,
                Description = x.Description,
                Value = x.Value * factor,
            }).ToList();
        }

        public List<AccountType> GetPossibleAccountTypes()
        {
            if (ledgerValue < 0) return new List<AccountType>() { AccountType.Asset};
            else return new List<AccountType>(){AccountType.Equity, AccountType.Liability};
        }
        
        public string GetAccountType()
        {
            if (ledgerValue > 0) return "Equity or Liability";
            else return "Asset";
        }
    }
}