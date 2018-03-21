using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.ExcelGateway
{
    public static class BalanceSheetStatementsCleaner
    {
        public static List<TrimmedBalanceSheetStatement> RemovedInvalidBalanceSheetStatements(
            List<BalanceSheetStatement> statements
            , ILogger logger)
        {
            var mismatchedStatements = statements.Where(x => !AccountClassifer.IsRealLedger(x.Account)).ToList();
            statements.RemoveAll(x => mismatchedStatements.Contains(x));

            if(mismatchedStatements.Any()) logger.Log(MessageType.Warning, "Previous balance sheet statement(s) have been trimmed. Please verify");

            return mismatchedStatements.Select(x => new TrimmedBalanceSheetStatement(x, "Invalid account name"))
                .ToList();
        }
    }
}