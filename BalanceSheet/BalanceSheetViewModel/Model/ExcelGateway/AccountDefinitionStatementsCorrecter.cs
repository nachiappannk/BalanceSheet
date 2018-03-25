using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.ExcelGateway
{
    public static class AccountDefinitionStatementsCorrecter
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
                    if (statement.AccountType != AccountType.Asset)
                    {
                        logger.Log(MessageType.Warning, "From the balance sheet " + name + " looks like an asset.", 
                            "Account definition is modified");
                        statement.AccountType = AccountType.Asset;

                    }

                }
                else
                {
                    if (statement.AccountType != AccountType.Liability || statement.AccountType != AccountType.Equity)
                        logger.Log(MessageType.Warning, "From the balance sheet " + name + " looks like an liability or equity" ,
                            "Account definition is modified");
                    statement.AccountType = AccountType.Liability;
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
}