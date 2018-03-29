using System;
using System.IO;
using System.Reflection;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

namespace Nachiappan.BalanceSheetViewModel
{
    public class AboutApplicationWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public InteractionRequest<FileSaveAsNotification> FileSaveAsInteractionRequest { get; private set; }

        public DelegateCommand SaveSampleBalanceSheetCommand { get; set; }
        public DelegateCommand SaveSampleJournalCommand { get; set; }
        public DelegateCommand SaveHelpDocumentCommand { get; set; }

        public DelegateCommand SaveAccountDefinitionCommand { get; set; }


        public AboutApplicationWorkFlowStepViewModel(Action nextStep)
        {
            Name = "About Balance Sheet";

            GoToPreviousCommand = new DelegateCommand(() => { }, () => false);
            GoToNextCommand = new DelegateCommand(nextStep, ()=> true);

            FileSaveAsInteractionRequest = new InteractionRequest<FileSaveAsNotification>();

            SaveSampleBalanceSheetCommand = new DelegateCommand(SaveSampleBalanceSheet);
            SaveSampleJournalCommand = new DelegateCommand(SaveJournal);
            SaveHelpDocumentCommand = new DelegateCommand(SaveHelpDocument);
            SaveAccountDefinitionCommand = new DelegateCommand(SaveAccountDefinition);

        }

        private void SaveHelpDocument()
        {
            SaveFile("Save Help Document",
                "HelpDocument",
                ".docx",
                "Excel File (.docx)|*.docx",
                "Nachiappan.BalanceSheetViewModel.Docs.HelpDocument.docx");
        }

        private void SaveSampleBalanceSheet()
        {

            SaveFile("Save Sample Balance Sheet",
                "BalanceSheetFormat",
                ".xlsx",
                "Excel File (.xlsx)|*.xlsx",
                "Nachiappan.BalanceSheetViewModel.Docs.PreviousBalanceSheetTemplate.xlsx");
        }

        private void SaveJournal()
        {

            SaveFile("Save Sample Journal",
                "JournalFormat",
                ".xlsx",
                "Excel File (.xlsx)|*.xlsx",
                "Nachiappan.BalanceSheetViewModel.Docs.CurrentJournalTemplate.xlsx");
        }

        private void SaveAccountDefinition()
        {

            SaveFile("Save Account Definition",
                "AccountDefinition",
                ".xlsx",
                "Excel File (.xlsx)|*.xlsx",
                "Nachiappan.BalanceSheetViewModel.Docs.AccountDefinitionFormat.xlsx");
        }

        private void SaveFile(string saveFileTitle, string defaultFileName, string outputFileExtention, string filter,
            string resourceName)
        {
            var file = new FileSaveAsNotification()
            {
                Title = saveFileTitle,
                DefaultFileName = defaultFileName,
                OutputFileExtention = outputFileExtention,
                OutputFileExtentionFilter = filter,
            };
            FileSaveAsInteractionRequest.Raise(file);
            if (file.FileSaved)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) throw new Exception("Unable to get the file");
                using (FileStream output = new FileStream(file.OutputFileName, FileMode.Create))
                {
                    stream.CopyTo(output);
                }
            }
        }
    }
}