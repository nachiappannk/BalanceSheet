using Prism.Commands;
using System;
using System.Collections.Generic;

namespace Nachiappan.BalanceSheetViewModel
{
    public class InputWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private readonly DataStore _dataStore;
        private readonly Action _nextStep;
        private readonly Action _previousStep;

        public InputWorkFlowStepViewModel(DataStore dataStore, Action nextStep, Action previousStep)
        {
            _dataStore = dataStore;
            _nextStep = nextStep;
            _previousStep = previousStep;
            Name = "Input";



            JournalSelectorViewModel = new ExcelSheetSelectorViewModel();
            JournalSelectorViewModel.Title = "Please provide the journal";
            PreviousBalanceSheetSelectorViewModel = new ExcelSheetSelectorViewModel();
            PreviousBalanceSheetSelectorViewModel.Title = "Please provide the previous period balance sheet";

            JournalSelectorViewModel.ValidityChanged += RaiseCanExecuteChanged;

            PreviousBalanceSheetSelectorViewModel.ValidityChanged += RaiseCanExecuteChanged;

            GoToPreviousCommand = new DelegateCommand(_previousStep , () => true);
            GoToNextCommand = new DelegateCommand(GoToNext, CanGoToNext);
        }
        
        private void RaiseCanExecuteChanged()
        {
            GoToNextCommand.RaiseCanExecuteChanged();
        }
        
        public ExcelSheetSelectorViewModel JournalSelectorViewModel { get; set; }
        public ExcelSheetSelectorViewModel PreviousBalanceSheetSelectorViewModel { get; set; }

        private DateTime? _accountingPeriodStartDate;
        public DateTime? AccountingPeriodStartDate
        {
            get { return _accountingPeriodStartDate; }
            set
            {
                if (_accountingPeriodStartDate != value)
                {
                    _accountingPeriodStartDate = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime? _accountingPeriodEndDate;
        public DateTime? AccountingPeriodEndDate
        {
            get { return _accountingPeriodEndDate; }
            set
            {
                if (_accountingPeriodEndDate != value)
                {
                    _accountingPeriodEndDate = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        bool CanGoToNext()
        {
            if (!JournalSelectorViewModel.IsValid) return false;
            if (!PreviousBalanceSheetSelectorViewModel.IsValid) return false;
            if (AccountingPeriodEndDate == null) return false;
            if (AccountingPeriodStartDate == null) return false;
            return true;
        }

        void GoToNext()
        {
            if (!AccountingPeriodEndDate.HasValue) return;
            if (!AccountingPeriodStartDate.HasValue) return;


            var input = new InputForBalanceSheetComputation();
            input.AccountingPeriodEndDate = AccountingPeriodEndDate.Value;
            input.AccountingPeriodStartDate = AccountingPeriodStartDate.Value;
            input.PreviousBalanceSheetFileName = PreviousBalanceSheetSelectorViewModel.InputFileName;
            input.PreviousBalanceSheetSheetName = PreviousBalanceSheetSelectorViewModel.SelectedSheet;
            input.CurrentJournalFileName = JournalSelectorViewModel.InputFileName;
            input.CurrentJournalSheetName = JournalSelectorViewModel.SelectedSheet;
            _dataStore.PutPackage(input);

            _nextStep.Invoke();

        }
    }
}