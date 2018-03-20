using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public class OptionsWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public List<LedgerOptionViewModel> LedgerOptions { get; set; }

        public OptionsWorkFlowStepViewModel(DataStore dataStore, Action goToPreviousStep, 
            Action goToNextStep)
        {
            GoToNextCommand = new DelegateCommand(goToNextStep);
            GoToPreviousCommand = new DelegateCommand(goToPreviousStep);
            Name = "Options";

            var ledgers = dataStore.GetPackage<List<ILedger>>(WorkFlowViewModel.LedgersPackageDefinition);

            var optionLedgers = ledgers
                .Where(x => x.GetPossibleLedgerTypes().Count > 1)
                .ToList();

            var optionDictionary = new Dictionary<string, LedgerType>();

            if (dataStore.IsPackageStored(WorkFlowViewModel.LedgerNameToTypeMapPackageDefinition))
            {
                optionDictionary = dataStore.GetPackage(WorkFlowViewModel.LedgerNameToTypeMapPackageDefinition);
            }
            else
            {
                dataStore.PutPackage(optionDictionary, WorkFlowViewModel.LedgerNameToTypeMapPackageDefinition);
            }

            LedgerOptions = optionLedgers.Select(y => new LedgerOptionViewModel(y, optionDictionary)).ToList();
        }
    }


    public class LedgerOptionViewModel
    {
        private readonly ILedger _ledger;
        private readonly Dictionary<string, LedgerType> _optionDictionary;

        public LedgerOptionViewModel(ILedger ledger, Dictionary<string, LedgerType> optionDictionary)
        {
            _ledger = ledger;
            _optionDictionary = optionDictionary;
            LedgerTypes = _ledger.GetPossibleLedgerTypes();
            Name = _ledger.GetPrintableName();
            if (!_optionDictionary.ContainsKey(Name))
            {
                _optionDictionary.Add(Name, LedgerTypes.ElementAt(0));
            }
        }

        public string Name { get; set; }

        public List<LedgerType> LedgerTypes { get; set; }


        public LedgerType LedgerType
        {
            get { return _optionDictionary[Name]; }
            set { _optionDictionary[Name] = value; }
        }
    }
}