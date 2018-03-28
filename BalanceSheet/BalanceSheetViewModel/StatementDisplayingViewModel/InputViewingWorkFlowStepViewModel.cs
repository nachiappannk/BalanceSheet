using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Statements;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel
{
    public class InputViewingWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public List<DisplayableJournalStatement> JournalStatements { get; set; }
        public List<DisplayableTrimmedJournalStatement> TrimmedJournalStatements { get; set; }
        public List<DisplayableBalanceSheetStatement> PreviousBalanceSheetStatements { get; set; }
        public List<DisplayableTrimmedBalanceSheetStatement> TrimmedBalanceSheetStatements { get; set; }
        public List<DisplayableCorrectedAccountDefintionStatement> CorrectedAccountDefintionStatements { get; set; }
        public List<DisplayableAccountDefintionStatement> AccountDefintionStatements { get; set; }

        public bool IsTrimmedBalanceSheetJournalVisible
        {
            get { return TrimmedBalanceSheetStatements.Any(); }
        }

        public bool IsTrimmedJournalVisible
        {
            get { return TrimmedJournalStatements.Any(); }
        }

        public bool IsCorrectedAccountDefinitionsVisible
        {
            get { return CorrectedAccountDefintionStatements.Any(); }
        }

        public InputViewingWorkFlowStepViewModel(DataStore dataStore, Action goBackAction)
        {
            GoToPreviousCommand = new DelegateCommand(goBackAction);
            GoToNextCommand = new DelegateCommand(() => { }, () => false);
            Name = "View Input Statements";

            var journalStatements = 
                dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion)
                    .MakeAccountPrintable(dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);
            ;
            JournalStatements = journalStatements.Select(x => new DisplayableJournalStatement(x)).ToList();

            var correctedJournalStatements =
                dataStore.GetPackage(WorkFlowViewModel.TrimmedJournalStatementsPackageDefintion)
                    .MakeAccountPrintable(dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);
            ;

            TrimmedJournalStatements = correctedJournalStatements.Select(x => new DisplayableTrimmedJournalStatement(x))
                .ToList();

            var previousBalanceSheetStatements =
                dataStore.GetPackage(WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition)
                    .MakeAccountPrintable(dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);
            ;
            PreviousBalanceSheetStatements = previousBalanceSheetStatements
                .Select(x => new DisplayableBalanceSheetStatement(x)).ToList();


            var correctedBalanceSheetStatements =
                dataStore.GetPackage(WorkFlowViewModel.TrimmedPreviousBalanceSheetStatements)
                    .MakeAccountPrintable(dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);
            
            TrimmedBalanceSheetStatements = correctedBalanceSheetStatements
                .Select(x => new DisplayableTrimmedBalanceSheetStatement(x)).ToList();

            var accountDefinitionStatements =
                dataStore.GetPackage(WorkFlowViewModel.InputAccountDefinitionPackageDefinition)
                    .MakeAccountPrintable(dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition)
                    .MakeRecipientAccountPrintable(dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);


            AccountDefintionStatements = accountDefinitionStatements
                .Select(x => new DisplayableAccountDefintionStatement(x)).ToList();


            var correctedAccountDefinitionStatements =
                dataStore.GetPackage(WorkFlowViewModel.CorrectedAccountDefinitionPackageDefinition)
                    .MakeAccountPrintable(dataStore,
                        WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);

            CorrectedAccountDefintionStatements = correctedAccountDefinitionStatements
                .Select(x => new DisplayableCorrectedAccountDefintionStatement(x)).ToList();

        }
    }
}