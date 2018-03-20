using System;
using System.Collections.Generic;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Ledger
{
    public interface ILedger
    {
        string GetPrintableName();
        void PostTransaction(DateTime date, string statement, double value);
        double GetLedgerValue();
        List<LedgerStatement> GetLedgerStatements();
        List<LedgerType> GetPossibleLedgerTypes();
        List<LedgerStatement> GetLedgerStatements(LedgerType ledgerType);
        string GetLedgerType();
    }
}