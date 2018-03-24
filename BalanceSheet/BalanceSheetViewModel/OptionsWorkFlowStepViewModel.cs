using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public class OptionsWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public List<AccountRelationViewModel> LedgerOptions { get; set; }

        public OptionsWorkFlowStepViewModel(DataStore dataStore, Action goToPreviousStep, 
            Action goToNextStep)
        {


            var input = dataStore.GetPackage(WorkFlowViewModel.InputParametersPackageDefinition);
            var journalStatements = dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion);
            var previousBalanceSheetStatements =
                dataStore.GetPackage(WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition);


            GeneralAccount generalAccount = new GeneralAccount(input.AccountingPeriodStartDate, input.AccountingPeriodEndDate,
                previousBalanceSheetStatements, journalStatements);

            dataStore.PutPackage(generalAccount.GetAllAccounts(), WorkFlowViewModel.AccountsPackageDefinition);
            dataStore.PutPackage(generalAccount.GetTrialBalanceStatements(), WorkFlowViewModel.TrialBalanceStatementsPackageDefinition);
            dataStore.PutPackage(generalAccount.GetBalanceSheetStatements(), WorkFlowViewModel.BalanceSheetStatementsPackageDefinition);





            GoToNextCommand = new DelegateCommand(goToNextStep);
            GoToPreviousCommand = new DelegateCommand(goToPreviousStep);
            Name = "Account type selection/verification";

            var ledgers = dataStore.GetPackage(WorkFlowViewModel.AccountsPackageDefinition);

            var optionLedgers = ledgers
                .Where(x => x.GetPossibleAccountTypes().Count > 1)
                .ToList();

            var optionDictionary = new Dictionary<string, AccountType>();

            if (dataStore.IsPackageStored(WorkFlowViewModel.AccountNameToTypeMapPackageDefinition))
            {
                optionDictionary = dataStore.GetPackage(WorkFlowViewModel.AccountNameToTypeMapPackageDefinition);
            }
            else
            {
                dataStore.PutPackage(optionDictionary, WorkFlowViewModel.AccountNameToTypeMapPackageDefinition);
            }

            LedgerOptions = optionLedgers.Select(y => new AccountRelationViewModel(y, optionDictionary)).ToList();
        }
    }


    public class AccountRelationViewModel
    {
        private readonly IAccount _account;
        private readonly Dictionary<string, AccountType> _optionDictionary;

        public AccountRelationViewModel(IAccount account, Dictionary<string, AccountType> optionDictionary)
        {
            _account = account;
            _optionDictionary = optionDictionary;
            AccountTypes = _account.GetPossibleAccountTypes();
            Name = _account.GetPrintableName();
            if (!_optionDictionary.ContainsKey(Name))
            {
                _optionDictionary.Add(Name, AccountTypes.ElementAt(0));
            }
        }

        public string Name { get; set; }

        public List<AccountType> AccountTypes { get; set; }


        public AccountType AccountType
        {
            get
            {
                return _optionDictionary[Name];
            }
            set
            {
                _optionDictionary[Name] = value;
            }
        }
    }
}