using System;
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

    public static class AccountDefinitionCleaner
    {
        public static List<CorrectedAccountDefintionStatement> CorrectInvalidStatements(List<AccountDefintionStatement> statements, 
            List<BalanceSheetStatement> previousBalanceSheetStatements, List<JournalStatement> journalStatements, ILogger logger)
        {

            var result = new List<CorrectedAccountDefintionStatement>();
            result.AddRange(RemoveDuplicationDefinitions(statements, logger));
            result.AddRange(RemoveMoreThan2DegreeOnNotionalness(statements, logger));

            

            var journalAccountNames = journalStatements.Select(x => x.Account).ToList();
            var reason1 = "Added: Account definition was not found, but account was referred in journal";
            result.AddRange(AddMissingAccountAndGetCorrectedList(statements, journalAccountNames, reason1));


            var balanceSheetAccountNames = previousBalanceSheetStatements.Select(x => x.Account).ToList();
            var reason2 = "Added: Account definition was not found, but account was referred in balance sheet";
            result.AddRange(AddMissingAccountAndGetCorrectedList(statements, balanceSheetAccountNames, reason2));

            var statementsAsDictionary = statements.ToDictionary(x => x.Account, x => x);
            foreach (var previousBalanceSheetStatement in previousBalanceSheetStatements)
            {
                var name = previousBalanceSheetStatement.Account;
                var statement = statementsAsDictionary[name];
                if (previousBalanceSheetStatement.Value.IsZero()) continue;
                if (previousBalanceSheetStatement.Value < 0)
                {
                    if(statement.AccountType != AccountType.Asset)
                        logger.Log(MessageType.Warning,"From the balance sheet "+ name +" looks like an asset");
                }
                else
                {
                    if (statement.AccountType != AccountType.Liability || statement.AccountType != AccountType.Equity)
                        logger.Log(MessageType.Warning, "From the balance sheet " + name + " looks like an liability or equity");
                }
            }
            return result;
        }

        private static List<CorrectedAccountDefintionStatement> AddMissingAccountAndGetCorrectedList(List<AccountDefintionStatement> statements, List<string> accountNames, string reason)
        {
            var accountDefinitionNamesHashSet = statements.Select(x => x.Account).ToHashSet();

            var accountNamesToBeAdded = accountNames.Where(x => !accountDefinitionNamesHashSet.Contains(x))
                .ToList();


            var definitionsToBeAdded = accountNamesToBeAdded.Select(x =>
                new AccountDefintionStatement()
                {
                    Account = x,
                    AccountType = AccountType.Asset,
                    RecipientAccount = String.Empty
                }).ToList();

            statements.AddRange(definitionsToBeAdded);

            var result2 = definitionsToBeAdded.Select(x =>
                new CorrectedAccountDefintionStatement(x,
                    reason)).ToList();
            return result2;
        }

        private static IEnumerable<CorrectedAccountDefintionStatement> RemoveDuplicationDefinitions
            (List<AccountDefintionStatement> statements, ILogger logger)
        {
            var timesMap = statements.Select(x => x.Account).GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            var duplicateAccounts = timesMap.Where(x => x.Value > 1).Select(x => x.Key).ToList();
            var duplicateDefintions = statements.Where(x => duplicateAccounts.Contains(x.Account)).ToList();
            statements.RemoveAll(duplicateDefintions.Contains);
            var result = duplicateDefintions.Select(x => new CorrectedAccountDefintionStatement(x, "Removed: Duplication definitions"));
            return result;
        }

        private static List<CorrectedAccountDefintionStatement> RemoveMoreThan2DegreeOnNotionalness
            (List<AccountDefintionStatement> statements, ILogger logger)
        {
            var realAccountDefintions = statements.Where(x => x.RecipientAccount == String.Empty)
                .ToList();
            var realAccountsHashSet = realAccountDefintions.Select(x => x.Account).ToHashSet();
            
            var notionalAccountDefinitions =
                statements.Where(x => realAccountsHashSet.Contains(x.RecipientAccount)).ToList();

            var notionalAccountsHashSet = notionalAccountDefinitions.Select(x => x.Account).ToHashSet();

            var doubleNotionalAccountDefinitions =
                statements.Where(x => notionalAccountsHashSet.Contains(x.RecipientAccount)).ToList();


            var validStatements = realAccountDefintions.ToList();
            validStatements.AddRange(notionalAccountDefinitions);
            validStatements.AddRange(doubleNotionalAccountDefinitions);

            var validStatmentsHashSet = validStatements.ToHashSet();

            var invalidStatements = statements.Where(x => !validStatmentsHashSet.Contains(x)).ToList();
            statements.RemoveAll(invalidStatements);
            return invalidStatements.Select(x =>
                new CorrectedAccountDefintionStatement(x, "The degree of notionalness is more than 2")).ToList();

        }
    }

    public static class HashSetExtentions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> inputs)
        {
            return new HashSet<T>(inputs);
        }
    }

    public static class RemoveExtentions
    {
        public static void RemoveAll<T>(this List<T> source, IEnumerable<T> toBeRemovedItems)
        {
            var itemsToRemovedHashSet = toBeRemovedItems.ToHashSet();
            source.RemoveAll(x => itemsToRemovedHashSet.Contains(x));
        }
    }

}