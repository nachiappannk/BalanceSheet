using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.ExcelGateway
{
    public class JournalStatementsCleaner
    {
        public static List<TrimmedJournalStatement> RemoveInvalidJournalStatements(
            List<JournalStatement> journalStatements, DateTime startDate, DateTime endDate,
            Logger logger)
        {
            var tJournalStatements = RemoveOutOfDateStatements(journalStatements, startDate, endDate);
            tJournalStatements.AddRange(RemoveStatementsWithInvalidAccount(journalStatements));
            tJournalStatements.AddRange(RemoveStatementsWithInvalidDescription(journalStatements));
            ValidateTrimmedJournalStatements(tJournalStatements, logger);
            return tJournalStatements;
        }

        private static IEnumerable<TrimmedJournalStatement> RemoveStatementsWithInvalidDescription(
            List<JournalStatement> statements)
        {
            var filteredStatements2 = statements.Where(x => string.IsNullOrWhiteSpace(x.Description)).ToList();
            statements.RemoveAll(x => filteredStatements2.Contains(x));
            var trimmedJournalStatements =
                filteredStatements2.Select(x => new TrimmedJournalStatement(x, "The description is invalid"));
            return trimmedJournalStatements;
        }

        private static List<TrimmedJournalStatement> RemoveStatementsWithInvalidAccount(
            List<JournalStatement> statements)
        {
            var invalidAccountStatement = statements.Where(x =>
            {
                var isNominalAccount = AccountClassifer.IsNominalLedger(x.Account);
                var isRealAccount = AccountClassifer.IsRealLedger(x.Account);
                var isDoubleNominalAccount = AccountClassifer.IsDoubleNominalLedger(x.Account);
                return !isRealAccount && !isNominalAccount && !isDoubleNominalAccount;
            }).ToList();

            statements.RemoveAll(x => invalidAccountStatement.Contains(x));
            return invalidAccountStatement
                .Select(x => new TrimmedJournalStatement(x, "The account is invalid")).ToList();
        }

        private static void ValidateTrimmedJournalStatements
            (List<TrimmedJournalStatement> trimmedStatements, ILogger logger)
        {
            if (trimmedStatements.Any())
            {
                logger.Log(MessageType.Warning,
                    "The invalid journal statement(s) have been removed. Please check");
            }
        }

        private static List<TrimmedJournalStatement> RemoveOutOfDateStatements(List<JournalStatement> statements,
            DateTime startDate, DateTime endDate)
        {
            var statementsBeforePeriod = statements.Where(x => x.Date < startDate).ToList();
            statements.RemoveAll(x => statementsBeforePeriod.Contains(x));
            var statementsAfterPeriod = statements.Where(x => x.Date > endDate).ToList();
            statements.RemoveAll(x => statementsAfterPeriod.Contains(x));

            var trimmedStatements = statementsAfterPeriod
                .Select(x => new TrimmedJournalStatement(x, "After the end of  accounting period")).ToList();
            trimmedStatements.AddRange(statementsBeforePeriod.Select(x =>
                new TrimmedJournalStatement(x, "Before the start of accounting period")));

            return trimmedStatements;
        }
    }
}