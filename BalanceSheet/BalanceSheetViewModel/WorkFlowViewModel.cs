using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nachiappan.BalanceSheetViewModel.Annotations;

namespace Nachiappan.BalanceSheetViewModel
{
    public class WorkFlowViewModel : INotifyPropertyChanged
    {

        public const string InputJournalPackage = nameof(InputJournalPackage);
        public const string TrimmedJournalPackage = nameof(TrimmedJournalPackage);
        public const string PreviousBalanceSheetPacakge = nameof(PreviousBalanceSheetPacakge);
        public const string InputParametersPackage = nameof(InputParametersPackage);
        public const string TrialBalancePackage = nameof(TrialBalancePackage);
        public const string BalanceSheetPackage = nameof(BalanceSheetPackage);

        public event PropertyChangedEventHandler PropertyChanged;

        public WorkFlowStepViewModel CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                FirePropertyChanged();
            }
        }


        private DataStore _dataStore;
        private WorkFlowStepViewModel _currentStep;


        public WorkFlowViewModel()
        {
            _dataStore = new DataStore();

            GoToAboutApplicationStep();
        }


        private void GoToPrintStatementWorkFlowStep()
        {
            CurrentStep = new PrintOutputWorkFlowStepViewModel(_dataStore, GoToStatementVerifyingWorkFlowStep);
        }


        private void GoToStatementVerifyingWorkFlowStep()
        {
            CurrentStep = new StatementVerifyingWorkFlowStepViewModel(_dataStore, GoToProcessingStep, GoToPrintStatementWorkFlowStep);
        }


        private void GoToProcessingStep()
        {
            CurrentStep = new ProcessingWorkFlowStepViewModel(_dataStore, GoToInputStep, GoToStatementVerifyingWorkFlowStep);
        }

        private void GoToInputStep()
        {
            CurrentStep = new InputWorkFlowStepViewModel(_dataStore, GoToProcessingStep, GoToAboutApplicationStep);
        }

        private void GoToAboutApplicationStep()
        {
            CurrentStep = new AboutApplicationWorkFlowStepViewModel(GoToInputStep);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}