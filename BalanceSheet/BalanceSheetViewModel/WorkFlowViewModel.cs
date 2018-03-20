using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nachiappan.BalanceSheetViewModel.Annotations;
using Nachiappan.BalanceSheetViewModel.Model.Ledger;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel
{
    public class WorkFlowViewModel : INotifyPropertyChanged
    {
        public static readonly PackageDefinition<List<JournalStatement>> InputJournalStatementsPackageDefintion 
            = new PackageDefinition<List<JournalStatement>>(nameof(InputJournalStatementsPackageDefintion));

        public static readonly PackageDefinition<List<TrimmedJournalStatement>> TrimmedJournalStatementsPackageDefintion
            = new PackageDefinition<List<TrimmedJournalStatement>>(nameof(TrimmedJournalStatementsPackageDefintion));
        
        public static readonly PackageDefinition<List<BalanceSheetStatement>> PreviousBalanceSheetStatementsPackageDefinition = 
            new PackageDefinition<List<BalanceSheetStatement>>(nameof(PreviousBalanceSheetStatementsPackageDefinition));

        public static readonly PackageDefinition<List<BalanceSheetStatement>> BalanceSheetStatementsPackageDefinition =
            new PackageDefinition<List<BalanceSheetStatement>>(nameof(BalanceSheetStatementsPackageDefinition));

        public static readonly PackageDefinition<InputForBalanceSheetComputation> InputParametersPackageDefinition = 
            new PackageDefinition<InputForBalanceSheetComputation>(nameof(InputParametersPackageDefinition));

        public static readonly PackageDefinition<List<TrialBalanceStatement>> TrialBalanceStatementsPackageDefinition = 
            new PackageDefinition<List<TrialBalanceStatement>>(nameof(TrialBalanceStatementsPackageDefinition));

        public static readonly PackageDefinition<List<ILedger>> LedgersPackageDefinition = 
            new PackageDefinition<List<ILedger>>(nameof(LedgersPackageDefinition));

        public static readonly PackageDefinition<Dictionary<string, LedgerType>> LedgerNameToTypeMapPackageDefinition = 
            new PackageDefinition<Dictionary<string, LedgerType>>(nameof(LedgerNameToTypeMapPackageDefinition));


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
            CurrentStep = new InputWorkFlowStepViewModel(_dataStore, GoToProcessingStep, GoToAboutApplicationStep);
        }

        private void GoToProcessingStep()
        {
            CurrentStep = new ProcessingWorkFlowStepViewModel(_dataStore, GoToInputStep, GoToOptionsStep);
        }

        private void GoToOptionsStep()
        {
            CurrentStep = new OptionsWorkFlowStepViewModel(_dataStore, GoToProcessingStep, GoToStatementVerifyingWorkFlowStep);
        }

        private void GoToStatementVerifyingWorkFlowStep()
        {
            CurrentStep = new StatementVerifyingWorkFlowStepViewModel(_dataStore, GoToOptionsStep, GoToPrintStatementWorkFlowStep);
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