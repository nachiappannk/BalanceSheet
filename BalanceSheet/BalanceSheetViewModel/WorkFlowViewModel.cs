﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nachiappan.BalanceSheetViewModel.Annotations;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.Statements;
using Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel;

namespace Nachiappan.BalanceSheetViewModel
{
    public class WorkFlowViewModel : INotifyPropertyChanged
    {
        public static readonly PackageDefinition<List<JournalStatement>> InputJournalStatementsPackageDefintion 
            = new PackageDefinition<List<JournalStatement>>(nameof(InputJournalStatementsPackageDefintion));

        public static readonly PackageDefinition<List<CorrectedJournalStatement>> TrimmedJournalStatementsPackageDefintion
            = new PackageDefinition<List<CorrectedJournalStatement>>(nameof(TrimmedJournalStatementsPackageDefintion));

        public static readonly PackageDefinition<List<AccountDefintionStatement>> InputAccountDefinitionPackageDefinition
            = new PackageDefinition<List<AccountDefintionStatement>>(nameof(InputAccountDefinitionPackageDefinition));

        public static readonly PackageDefinition<List<BalanceSheetStatement>> PreviousBalanceSheetStatementsPackageDefinition = 
            new PackageDefinition<List<BalanceSheetStatement>>(nameof(PreviousBalanceSheetStatementsPackageDefinition));

        public static readonly PackageDefinition<List<BalanceSheetStatement>> BalanceSheetStatementsPackageDefinition =
            new PackageDefinition<List<BalanceSheetStatement>>(nameof(BalanceSheetStatementsPackageDefinition));

        public static readonly PackageDefinition<InputForBalanceSheetComputation> InputParametersPackageDefinition = 
            new PackageDefinition<InputForBalanceSheetComputation>(nameof(InputParametersPackageDefinition));

        public static readonly PackageDefinition<List<TrialBalanceStatement>> TrialBalanceStatementsPackageDefinition = 
            new PackageDefinition<List<TrialBalanceStatement>>(nameof(TrialBalanceStatementsPackageDefinition));

        public static readonly PackageDefinition<List<IAccount>> AccountsPackageDefinition = 
            new PackageDefinition<List<IAccount>>(nameof(AccountsPackageDefinition));

        public static readonly PackageDefinition<List<CorrectedBalanceSheetStatement>> TrimmedPreviousBalanceSheetStatements = 
            new PackageDefinition<List<CorrectedBalanceSheetStatement>>(nameof(TrimmedPreviousBalanceSheetStatements));

        public static readonly PackageDefinition<List<CorrectedAccountDefintionStatement>> CorrectedAccountDefinitionPackageDefinition = 
            new PackageDefinition<List<CorrectedAccountDefintionStatement>>(nameof(CorrectedAccountDefinitionPackageDefinition));

        public static readonly PackageDefinition<Dictionary<string,string>> DisplayableAccountNamesDictionaryPackageDefinition =
            new PackageDefinition<Dictionary<string, string>>(nameof(DisplayableAccountNamesDictionaryPackageDefinition));


        public static readonly PackageDefinition<List<AccountPrintOption>> AccountPrintOptionsPackageDefinition =
            new PackageDefinition<List<AccountPrintOption>>(nameof(AccountPrintOptionsPackageDefinition));


        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DataStore _dataStore;

        private WorkFlowStepViewModel _currentStep;
        public WorkFlowStepViewModel CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                FirePropertyChanged();
            }
        }
        
        public WorkFlowViewModel()
        {
            _dataStore = new DataStore();
            GoToAboutApplicationStep();
        }

        private void GoToAboutApplicationStep()
        {
            CurrentStep = new AboutApplicationWorkFlowStepViewModel(GoToInputStep);
        }

        private void GoToInputStep()
        {
            CurrentStep = new InputWorkFlowStepViewModel(_dataStore, GoToInputReadingStep, GoToAboutApplicationStep);
        }

        private void GoToInputReadingStep()
        {
            CurrentStep = new InputReadingWorkFlowStepViewModel(_dataStore, GoToInputStep, GoToStatementVerifyingWorkFlowStep, 
                SetCurrentStep);
        }

 
        private void SetCurrentStep(WorkFlowStepViewModel viewModel)
        {
            CurrentStep = viewModel;
        }


        private void GoToStatementVerifyingWorkFlowStep()
        {
            CurrentStep = new StatementVerifyingWorkFlowStepViewModel(_dataStore, GoToInputReadingStep, GoToPrintStatementWorkFlowStep);
        }

        private void GoToPrintStatementWorkFlowStep()
        {
            CurrentStep = new PrintOutputWorkFlowStepViewModel(_dataStore, GoToStatementVerifyingWorkFlowStep);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}