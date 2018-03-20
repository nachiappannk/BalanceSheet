using Prism.Commands;
using System;
using System.IO;
using System.Linq;

namespace Nachiappan.BalanceSheetViewModel
{
    public class InputWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private readonly DataStore _dataStore;
        private readonly Action _goToNextStep;
        private readonly Action _goToPreviousStep;

        public InputWorkFlowStepViewModel(DataStore dataStore, Action goToNextStep, Action goToPreviousStep)
        {
            _dataStore = dataStore;
            _goToNextStep = goToNextStep;
            _goToPreviousStep = goToPreviousStep;
            Name = "Input";



            JournalSelectorViewModel = new ExcelSheetSelectorViewModel();
            JournalSelectorViewModel.Title = "Please provide the journal";
            PreviousBalanceSheetSelectorViewModel = new ExcelSheetSelectorViewModel();
            PreviousBalanceSheetSelectorViewModel.Title = "Please provide the previous period balance sheet";

            
            JournalSelectorViewModel.ValidityChanged += RaiseCanExecuteChanged;
            PreviousBalanceSheetSelectorViewModel.ValidityChanged += RaiseCanExecuteChanged;

            GoToPreviousCommand = new DelegateCommand(_goToPreviousStep, () => true);
            GoToNextCommand = new DelegateCommand(GoToNext, CanGoToNext);


            if (dataStore.IsPackageStored(WorkFlowViewModel.InputParametersPackageDefinition))
            {
                var inputParameters = dataStore.GetPackage(WorkFlowViewModel.InputParametersPackageDefinition);

                SetExcelInputParameters(JournalSelectorViewModel, 
                    inputParameters.CurrentJournalFileName, inputParameters.CurrentJournalSheetName);

                SetExcelInputParameters(PreviousBalanceSheetSelectorViewModel,
                    inputParameters.PreviousBalanceSheetFileName, inputParameters.PreviousBalanceSheetSheetName);

                AccountingPeriodStartDate = inputParameters.AccountingPeriodStartDate;
                AccountingPeriodEndDate = inputParameters.AccountingPeriodEndDate;
            }
        }

        private static void SetExcelInputParameters(ExcelSheetSelectorViewModel excelSelectorViewModel,
            string fileName,
            string sheetName)
        {
            if (File.Exists(fileName))
            {
                excelSelectorViewModel.InputFileName = fileName;
                if (excelSelectorViewModel.SheetNames.Contains(sheetName))
                {
                    excelSelectorViewModel.SelectedSheet = sheetName;
                }
            }
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
            _dataStore.PutPackage(input ,WorkFlowViewModel.InputParametersPackageDefinition);

            _goToNextStep.Invoke();

        }
    }
}