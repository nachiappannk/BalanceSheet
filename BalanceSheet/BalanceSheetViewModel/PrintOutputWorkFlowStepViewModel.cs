using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
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
                //file.OutputFileName
                WriteJournal(file.OutputFileName);
            }
        }


        private void WriteJournal(String fileName)
        {

            var journalStatements = _dataStore.GetPackage<List<JournalStatement>>();
            JournalGateway gateway = new JournalGateway(fileName);
            gateway.WriteJournal(journalStatements);
        }       
    }
}