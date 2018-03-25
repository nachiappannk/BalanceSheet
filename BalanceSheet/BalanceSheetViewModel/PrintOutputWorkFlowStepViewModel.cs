using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using Nachiappan.BalanceSheetViewModel.Model;
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

        public InteractionRequest<FileSaveAsNotification> SaveOutputRequest { get; private set; }

        public PrintOutputWorkFlowStepViewModel(DataStore dataStore, Action goToPrevious)
        {
            _dataStore = dataStore;
            Name = "Save Output";
            GoToPreviousCommand = new DelegateCommand(goToPrevious);
            GoToNextCommand = new DelegateCommand(CloseApplication);
            SaveOutputCommand = new DelegateCommand(SaveOutput);
            SaveOutputRequest = new InteractionRequest<FileSaveAsNotification>();


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
            }
        }

        private void WriteTrialBalance(string outputFileName)
        {
            var headings = new List<string> {"S.No.", "Account", "Tag", "Credit", "Debit"};

            var trialBalanceStatements =
                _dataStore.GetPackage(WorkFlowViewModel.TrialBalanceStatementsPackageDefinition);

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
            var accountDefintionStatements = _dataStore.GetPackage(WorkFlowViewModel.InputAccountDefinitionPackageDefinition);
            AccountDefinitionGateway gateway = new AccountDefinitionGateway(outputFileName);
            gateway.WirteAccountDefinitions(accountDefintionStatements);
        }

        private void WritePreviousBalanceSheet(string outputFileName)
        {
            var balanceStatements = _dataStore.GetPackage(WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition);
            BalanceSheetGateway gateway = new BalanceSheetGateway(outputFileName);
            gateway.WriteBalanceSheet(balanceStatements, "PreviousBS");
        }

        private void WriteBalanceSheet(string outputFileName)
        {
            var balanceStatements = _dataStore.GetPackage(WorkFlowViewModel.BalanceSheetStatementsPackageDefinition);
            BalanceSheetGateway gateway = new BalanceSheetGateway(outputFileName);
            gateway.WriteBalanceSheet(balanceStatements, "BalanceSheet");
        }


        private void WriteJournal(String fileName)
        {
            var journalStatements = _dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion);
            JournalGateway gateway = new JournalGateway(fileName);
            gateway.WriteJournal(journalStatements);
        }       
    }
}