using Nachiappan.BalanceSheetViewModel.Model.Account;

namespace Nachiappan.BalanceSheetViewModel.Model
{
    public static class FinancialStatementsComputer
    {
        public static void ComputerFinanicalStatemments(DataStore dataStore)
        {
            var input = dataStore.GetPackage(WorkFlowViewModel.InputParametersPackageDefinition);
            var journalStatements = dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion);
            var previousBalanceSheetStatements =
                dataStore.GetPackage(WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition);
            var accountDefinitionStatement =
                dataStore.GetPackage(WorkFlowViewModel.InputAccountDefinitionPackageDefinition);

            var accountPrintableNamesLookup =
                dataStore.GetPackage(WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);

            GeneralAccount generalAccount = new GeneralAccount(input.AccountingPeriodStartDate, input.AccountingPeriodEndDate,
                previousBalanceSheetStatements, journalStatements, accountDefinitionStatement, accountPrintableNamesLookup);


            dataStore.PutPackage(generalAccount.GetAllAccounts(), WorkFlowViewModel.AccountsPackageDefinition);
            dataStore.PutPackage(generalAccount.GetTrialBalanceStatements(),
                WorkFlowViewModel.TrialBalanceStatementsPackageDefinition);
            dataStore.PutPackage(generalAccount.GetBalanceSheetStatements(),
                WorkFlowViewModel.BalanceSheetStatementsPackageDefinition);
        }
    }
}