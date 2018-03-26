using System;
using System.IO;
using System.Reflection;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

namespace Nachiappan.BalanceSheetViewModel
{
    public class AboutApplicationWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public InteractionRequest<FileSaveAsNotification> SampleBalanceSheetSaveAsRequest { get; private set; }
        public InteractionRequest<FileSaveAsNotification> SampleJournalSaveAsRequest { get; private set; }
        public InteractionRequest<FileSaveAsNotification> SampleAccountDefinitionSaveAsRequest { get; private set; }
        public InteractionRequest<FileSaveAsNotification> DocumentationSaveAsRequest { get; private set; }

        public DelegateCommand SaveSampleBalanceSheetCommand { get; set; }
        public DelegateCommand SaveSampleJournalCommand { get; set; }
        public DelegateCommand SaveHelpDocumentCommand { get; set; }

        public DelegateCommand SaveAccountDefinitionCommand { get; set; }


        public AboutApplicationWorkFlowStepViewModel(Action nextStep)
        {
            Name = "About Balance Sheet";

            GoToPreviousCommand = new DelegateCommand(() => { }, () => false);
            GoToNextCommand = new DelegateCommand(nextStep, ()=> true);

            SampleBalanceSheetSaveAsRequest = new InteractionRequest<FileSaveAsNotification>();
            SampleJournalSaveAsRequest = new InteractionRequest<FileSaveAsNotification>();
            DocumentationSaveAsRequest = new InteractionRequest<FileSaveAsNotification>();
            SampleAccountDefinitionSaveAsRequest = new InteractionRequest<FileSaveAsNotification>();

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
                "Nachiappan.BalanceSheetViewModel.Docs.HelpDocument.docx", DocumentationSaveAsRequest);
        }

        private void SaveSampleBalanceSheet()
        {

            SaveFile("Save Sample Balance Sheet",
                "BalanceSheetFormat",
                ".xlsx",
                "Excel File (.xlsx)|*.xlsx",
                "Nachiappan.BalanceSheetViewModel.Docs.PreviousBalanceSheetTemplate.xlsx", SampleBalanceSheetSaveAsRequest);
        }

        private void SaveJournal()
        {

            SaveFile("Save Sample Journal",
                "JournalFormat",
                ".xlsx",
                "Excel File (.xlsx)|*.xlsx",
                "Nachiappan.BalanceSheetViewModel.Docs.CurrentJournalTemplate.xlsx", SampleJournalSaveAsRequest);
        }

        private void SaveAccountDefinition()
        {

            SaveFile("Save Account Definition",
                "AccountDefinition",
                ".xlsx",
                "Excel File (.xlsx)|*.xlsx",
                "Nachiappan.BalanceSheetViewModel.Docs.AccountDefinitionFormat.xlsx", SampleAccountDefinitionSaveAsRequest);
        }

        private void SaveFile(string saveFileTitle, string defaultFileName, string outputFileExtention, string filter,
            string resourceName, InteractionRequest<FileSaveAsNotification> interactionRequest)
        {
            var file = new FileSaveAsNotification()
            {
                Title = saveFileTitle,
                DefaultFileName = defaultFileName,
                OutputFileExtention = outputFileExtention,
                OutputFileExtentionFilter = filter,
            };
            interactionRequest.Raise(file);
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