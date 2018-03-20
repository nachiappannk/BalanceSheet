using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Ledger
{
    public class NominalLedger : ILedger
    {
        private readonly string _accountName;
        private double ledgerValue = 0;

        private readonly List<LedgerStatement> _ledgerStatements = new List<LedgerStatement>();

        public List<LedgerStatement> GetLedgerStatements()
        {
            return _ledgerStatements.ToList();
        }

        public List<LedgerType> GetPossibleLedgerTypes()
        {
            return new List<LedgerType>(){LedgerType.Notional};
        }

        public List<LedgerStatement> GetLedgerStatements(LedgerType ledgerType)
        {
            return GetLedgerStatements();
        }

        public string GetLedgerType()
        {
            return "Nominal Account";
        }

        public NominalLedger(string accountName)
        {
            _accountName = accountName;
        }

        public string GetPrintableName()
        {
            return _accountName.Replace("/","-");
        }

        public void PostTransaction(DateTime date, string statement, double value)
        {
            var count = _ledgerStatements.Count + 1;
            _ledgerStatements.Add(new LedgerStatement() { Date = date, Description = statement, SerialNumber = count, Value = value });
            ledgerValue += value;
        }

        public double GetLedgerValue()
        {
            return ledgerValue;
        }
    }
}