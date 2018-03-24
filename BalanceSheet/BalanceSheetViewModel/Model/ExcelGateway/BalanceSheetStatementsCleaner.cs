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

            var trimmedStatements1 = RemoveDuplicateStatements(statements);

            var trimmedStatements2 = RemoveStatementsWithInvalidAccount(statements, logger);

            trimmedStatements2.AddRange(trimmedStatements1);

            if (trimmedStatements2.Any())
                logger.Log(MessageType.Warning, "Invalid statements from previous balance sheet are removed");
            return trimmedStatements2;
        }

        private static List<TrimmedBalanceSheetStatement> RemoveStatementsWithInvalidAccount(List<BalanceSheetStatement> statements, ILogger logger)
        {
            var mismatchedStatements = statements.Where(x => !AccountClassifer.IsRealLedger(x.Account)).ToList();
            statements.RemoveAll(x => mismatchedStatements.Contains(x));
            
            var z = mismatchedStatements.Select(x => new TrimmedBalanceSheetStatement(x, "Invalid account name"))
                .ToList();
            return z;
        }

        private static List<TrimmedBalanceSheetStatement> RemoveDuplicateStatements(List<BalanceSheetStatement> statements)
        {
            var accountToTimesDictionary =
                statements.Select(x => x.Account).GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            var invalidAccountNames = accountToTimesDictionary.Where(x => x.Value > 1).Select(x => x.Key).ToList();
            var statementsWhereAccountIsrepeated = statements.Where(x => invalidAccountNames.Contains(x.Account)).ToList();
            statements.RemoveAll(x => statementsWhereAccountIsrepeated.Contains(x));
            var trimmedStatements = statementsWhereAccountIsrepeated.Select(x =>
                new TrimmedBalanceSheetStatement(x, "The account entry is there multiple times")).ToList();
            return trimmedStatements;
        }
    }
}