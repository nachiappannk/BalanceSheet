using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.Excel;
using Nachiappan.BalanceSheetViewModel.Model.ExcelGateway;
using Nachiappan.BalanceSheetViewModel.Model.Statements;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

namespace Nachiappan.BalanceSheetViewModel
{
    public class PrintOutputWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private readonly DataStore _dataStore;
        public DelegateCommand SaveOutputCommand { get; set; }

        public List<AccountPrintOption> AccountPrintOptions { get; set; }

        public InteractionRequest<FileSaveAsNotification> SaveOutputRequest { get; private set; }

        public PrintOutputWorkFlowStepViewModel(DataStore dataStore, Action goToPrevious)
        {
            _dataStore = dataStore;
            Name = "Save Output";
            GoToPreviousCommand = new DelegateCommand(goToPrevious);
            GoToNextCommand = new DelegateCommand(CloseApplication);
            SaveOutputCommand = new DelegateCommand(SaveOutput);
            SaveOutputRequest = new InteractionRequest<FileSaveAsNotification>();
            

            

            if (dataStore.IsPackageStored(WorkFlowViewModel.AccountPrintOptionsPackageDefinition))
            {
                AccountPrintOptions = dataStore.GetPackage(WorkFlowViewModel.AccountPrintOptionsPackageDefinition);
            }
            else
            {
                var accountDefinitions = dataStore.GetPackage(WorkFlowViewModel.InputAccountDefinitionPackageDefinition);
                AccountPrintOptions =
                    accountDefinitions.Select(x => new AccountPrintOption() { Name = x.Account }).ToList();
                dataStore.PutPackage(AccountPrintOptions, WorkFlowViewModel.AccountPrintOptionsPackageDefinition);

            }
        }

        private void CloseApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void SaveOutput()
        {
            SaveFile("Save Output Document",
                "FinancialReport",
                ".xlsx",
                "Excel File (.xlsx)|*.xlsx");
        }

        private void SaveFile(string saveFileTitle, string defaultFileName, string outputFileExtention, string filter)
        {
            var file = new FileSaveAsNotification()
            {
                Title = saveFileTitle,
                DefaultFileName = defaultFileName,
                OutputFileExtention = outputFileExtention,
                OutputFileExtentionFilter = filter,
            };

            SaveOutputRequest.Raise(file);
            if (file.FileSaved)
            {
                var outputFileName = file.OutputFileName;
                if (File.Exists(outputFileName)) File.Delete(outputFileName);
                WriteJournal(outputFileName);
                WritePreviousBalanceSheet(outputFileName);
                WriteBalanceSheet(outputFileName);
                WriteTrialBalance(outputFileName);
                WriteAccountDefinitions(outputFileName);
                WriteAccounts(outputFileName);
            }
        }

        private void WriteAccounts(string outputFileName)
        {
            var accountsToBePrinted =
                AccountPrintOptions.Where(x => x.IsPrintingNecessary).Select(x => x.Name).ToList();

            var accounts = _dataStore.GetPackage(WorkFlowViewModel.AccountsPackageDefinition);
            var accountsDictionary = accounts.ToDictionary(x => x.GetName(), x => x);
            foreach (var accountToBePrinted in accountsToBePrinted)
            {
                WriteAccount(accountsDictionary[accountToBePrinted], outputFileName);
            }
        }

        private void WriteAccount(IAccount account, string outputFileName)
        {
            var statements = account.GetAccountStatements();
            var headings = new List<string> {"S.No.", "Date", "Account", "Credit", "Debit", "Balance" };
            using (var writer = new ExcelSheetWriter(outputFileName, "Acc-"+account.GetName()))
            {
                var index = 0;
                writer.Write(index++, headings.ToArray<object>());
                writer.SetColumnsWidth(6, 12, 45, 12, 12, 12);
                writer.ApplyHeadingFormat(headings.Count);
                writer.WriteList(index, statements, (b, rowIndex) => new object[]
                {
                    b.SerialNumber,
                    b.Date,
                    b.Description,
                    b.GetCreditValue(),
                    b.GetDebitValue(),
                    b.RunningTotaledValue,
                });
                index = index + 1 + statements.Count;
                writer.Write(index, new object[] { "", "Total", "", statements.GetCreditTotal(),
                    statements.GetDebitTotal(), statements.GetTotal()});

            }
        }

        private void WriteTrialBalance(string outputFileName)
        {
            var headings = new List<string> {"S.No.", "Account", "Tag", "Credit", "Debit"};

            var trialBalanceStatements =
                _dataStore.GetPackage(WorkFlowViewModel.TrialBalanceStatementsPackageDefinition)
                    .MakeAccountPrintable(_dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);

            using (var writer = new ExcelSheetWriter(outputFileName, "TrialBalance"))
            {
                var index = 0;
                writer.Write(index++, headings.ToArray<object>());
                writer.SetColumnsWidth(6, 45, 12, 12, 12, 12);
                writer.ApplyHeadingFormat(headings.Count);
                writer.WriteList(index, trialBalanceStatements, (b, rowIndex) => new object[]
                {
                    rowIndex - 1,
                    b.Account,
                    b.Tag,
                    b.GetCreditValue(),
                    b.GetDebitValue(),
                });
                index = index + 1 + trialBalanceStatements.Count;
                writer.Write(index, new object[] { "", "Total", "", trialBalanceStatements.GetCreditTotal(),
                    trialBalanceStatements.GetDebitTotal(), trialBalanceStatements.GetTotal()});

            }
        }


        private void WriteAccountDefinitions(string outputFileName)
        {
            var accountDefintionStatements = _dataStore.GetPackage(WorkFlowViewModel.InputAccountDefinitionPackageDefinition)
            .MakeAccountPrintable(_dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition)
                .MakeRecipientAccountPrintable(_dataStore,
                WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);


            AccountDefinitionGateway gateway = new AccountDefinitionGateway(outputFileName);
            gateway.WirteAccountDefinitions(accountDefintionStatements);
        }

        private void WritePreviousBalanceSheet(string outputFileName)
        {
            var balanceStatements = _dataStore.GetPackage(WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition)
                .MakeAccountPrintable(_dataStore,
                    WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);

            BalanceSheetGateway gateway = new BalanceSheetGateway(outputFileName);
            gateway.WriteBalanceSheet(balanceStatements, "PreviousBS");
        }

        private void WriteBalanceSheet(string outputFileName)
        {
            var balanceStatements = _dataStore.GetPackage(WorkFlowViewModel.BalanceSheetStatementsPackageDefinition)
                .MakeAccountPrintable(_dataStore,
                    WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);
            ;
            BalanceSheetGateway gateway = new BalanceSheetGateway(outputFileName);
            gateway.WriteBalanceSheet(balanceStatements, "BalanceSheet");
        }


        private void WriteJournal(String fileName)
        {
            var journalStatements = _dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion)
                .MakeAccountPrintable(_dataStore,
                    WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);

            JournalGateway gateway = new JournalGateway(fileName);
            gateway.WriteJournal(journalStatements);
        }       
    }

    public class AccountPrintOption
    {
        public string Name { get; set; }
        public bool IsPrintingNecessary { get; set; }
    }
}