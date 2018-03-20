using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Ledger
{
    public class RealLedger : ILedger
    {
        private readonly string _accountName;
        private double ledgerValue = 0;

        private List<LedgerStatement> ledgerStatements = new List<LedgerStatement>();

        public RealLedger(string accountName, DateTime openingDate, double value)
        {
            _accountName = accountName;
            ledgerStatements.Add(new LedgerStatement()
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

       
        public void PostTransaction(DateTime date, string statement, double value)
        {
            var count = ledgerStatements.Count + 1;
            ledgerStatements.Add(new LedgerStatement() { Date = date, Description = statement, SerialNumber = count, Value = value });
            ledgerValue += value;
        }

        public double GetLedgerValue()
        {
            return ledgerValue;
        }

        public List<LedgerStatement> GetLedgerStatements()
        {
            return ledgerStatements.ToList();
        }

        public List<LedgerStatement> GetLedgerStatements(LedgerType ledgerType)
        {
            bool isInversionRequired = false;
            if (GetPossibleLedgerTypes().Contains(ledgerType))
            {
                if (ledgerType == LedgerType.Asset) isInversionRequired = true;
                if (ledgerType == LedgerType.Liability) isInversionRequired = true;
                if (ledgerType == LedgerType.Equity) isInversionRequired = false;
            }

            var ledgerStatements = GetLedgerStatements();

            double factor = 1;
            if(isInversionRequired) factor = -1;
            return ledgerStatements.Select(x => new LedgerStatement()
            {
                SerialNumber = x.SerialNumber,
                Date = x.Date,
                Description = x.Description,
                Value = x.Value * factor,
            }).ToList();
        }

        public List<LedgerType> GetPossibleLedgerTypes()
        {
            if (ledgerValue < 0) return new List<LedgerType>() { LedgerType.Asset};
            else return new List<LedgerType>(){LedgerType.Equity, LedgerType.Liability};
        }
        
        public string GetLedgerType()
        {
            if (ledgerValue > 0) return "Equity or Liability";
            else return "Asset";
        }
    }
}