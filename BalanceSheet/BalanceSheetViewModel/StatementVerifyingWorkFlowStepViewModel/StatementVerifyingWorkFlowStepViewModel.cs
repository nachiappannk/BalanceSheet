﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.Statements;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel.StatementVerifyingWorkFlowStepViewModel
{
    public class StatementVerifyingWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public List<DisplayableBalanceSheetStatement> PreviousBalanceSheetStatements { get; set; }
        public List<DisplayableTrimmedBalanceSheetStatement> TrimmedBalanceSheetStatements { get; set; }
        public List<DisplayableBalanceSheetStatement> BalanceSheetStatements { get; set; }
        public List<DisplayableJournalStatement> JournalStatements { get; set; }
        public List<DisplayableTrimmedJournalStatement> TrimmedJournalStatements { get; set; }
        public List<DisplayableTrialBalanceStatement> TrialBalanceStatements { get; set; }

        public bool IsTrimmedJournalVisible { get; set; }

        public bool IsTrimmedBalanceSheetJournalVisible { get; set; }

        private string _selectedLedgerName;
        private readonly Dictionary<string, IAccount> _accounts;
        private List<DisplayableAccountStatement> _ledgerStatements;
        private string _ledgerType;
        private Dictionary<string, AccountType> _ledgerTypes;
        private SelectedAccountViewModel _selectedAccountViewModel;

        public StatementVerifyingWorkFlowStepViewModel(DataStore dataStore, Action goToPreviousStep, 
            Action goToNextStep)
        {

            _ledgerTypes = dataStore.GetPackage(WorkFlowViewModel.AccountNameToTypeMapPackageDefinition);

            GoToPreviousCommand = new DelegateCommand(goToPreviousStep);
            GoToNextCommand = new DelegateCommand(goToNextStep);
            Name = "Verify Input/Output Statements";

            PreviousBalanceSheetStatements = GetBalanceSheetStatements(dataStore, WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition);
            BalanceSheetStatements = GetBalanceSheetStatements(dataStore, WorkFlowViewModel.BalanceSheetStatementsPackageDefinition);

            JournalStatements = GetInputJournalStatement(dataStore);
            TrimmedJournalStatements = GetTrimmedStatements(dataStore);
            TrialBalanceStatements = GetTrailBalanceStatements(dataStore);
            TrimmedBalanceSheetStatements = GetTrimmedBalanceSheetStatements(dataStore);

            IsTrimmedJournalVisible = TrimmedJournalStatements.Any();
            IsTrimmedBalanceSheetJournalVisible = TrimmedBalanceSheetStatements.Any();

            _accounts = CreateAccountDictionary(dataStore);



            LedgerNames = _accounts.Select(x => x.Key).ToList();
            SelectedLedgerName = LedgerNames.ElementAt(0);

            
        }

        private static Dictionary<string, IAccount> CreateAccountDictionary(DataStore dataStore)
        {
            var allLedgers = dataStore.GetPackage(WorkFlowViewModel.AccountsPackageDefinition);
            var dictionary = allLedgers.ToDictionary(x => x.GetPrintableName(), x => x);
            return dictionary;
        }

        private static List<DisplayableTrimmedBalanceSheetStatement> GetTrimmedBalanceSheetStatements(DataStore dataStore)
        {
            return dataStore
                .GetPackage(WorkFlowViewModel.TrimmedPreviousBalanceSheetStatements)
                .Select(x => new DisplayableTrimmedBalanceSheetStatement(x)).ToList();
        }

        private List<DisplayableTrialBalanceStatement> GetTrailBalanceStatements(DataStore dataStore)
        {
            return dataStore
                .GetPackage(WorkFlowViewModel.TrialBalanceStatementsPackageDefinition)
                .Select(x => new DisplayableTrialBalanceStatement(x)).ToList();
        }

        private List<DisplayableTrimmedJournalStatement> GetTrimmedStatements(DataStore dataStore)
        {
            var journalStatements = dataStore.GetPackage(WorkFlowViewModel.TrimmedJournalStatementsPackageDefintion);
            return journalStatements.Select(x => new DisplayableTrimmedJournalStatement(x)).ToList();
        }

        private List<DisplayableJournalStatement> GetInputJournalStatement(DataStore dataStore)
        {
            var journalStatements = dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion);
            return journalStatements.Select(x =>
                new DisplayableJournalStatement(x)).ToList();
        }

        private List<DisplayableBalanceSheetStatement> GetBalanceSheetStatements(DataStore dataStore, PackageDefinition<List<BalanceSheetStatement>> packageDefinition)
        {
            var statements = dataStore.GetPackage(packageDefinition);
            var displayableStatements = statements
                .Select(x => new DisplayableBalanceSheetStatement(x))
                .ToList();
            return displayableStatements;
        }


        public SelectedAccountViewModel SelectedAccountViewModel
        {
            get { return _selectedAccountViewModel; }
            set
            {
                _selectedAccountViewModel = value;
                FirePropertyChanged();
            }
        }


        public List<string> LedgerNames { get; set; }

        public string SelectedLedgerName
        {
            get { return _selectedLedgerName; }
            set
            {
                _selectedLedgerName = value;
                if (_accounts.ContainsKey(value))
                {
                    SelectedAccountViewModel = new SelectedAccountViewModel(_accounts, value, _ledgerTypes);
                }
                FirePropertyChanged();
            }
        }

        
    }

    public class SelectedAccountViewModel
    {
        public SelectedAccountViewModel(IDictionary<string, IAccount> accounts, string selectedAccount, 
            IDictionary<string,AccountType> accountTypes)
        {
            var account = accounts[selectedAccount];
            var accountType = GetAccountType(account, accountTypes);
            var statements = account.GetAccountStatements(accountType);
            AccountName = selectedAccount;
            AccountStatements = statements.Select(x => new DisplayableAccountStatement(x)).ToList();
            AccountType = accountType.ToString();
            var overAllMessage = GetOverallMessage(accountType, account);

            OverAllMessage = overAllMessage;
        }

        private static string GetOverallMessage(AccountType accountType, IAccount account)
        {
            switch (accountType)
            {
                case Model.Account.AccountType.Notional:
                    return "Notional Account";
                case Model.Account.AccountType.Equity:
                    return "The total equity contribution from this account is " + account.GetAccountValue().ToString("N2");
                case Model.Account.AccountType.Asset:
                    return "The investment is " + (account.GetAccountValue() * -1).ToString("N2"); ;
                case Model.Account.AccountType.Liability:
                default:
                    return "The borrowing is " + account.GetAccountValue().ToString("N2"); ;

            }
        }

        private AccountType GetAccountType(IAccount ledger, IDictionary<string, AccountType> preferenceAccountTypes)
        {
            var possibleAccountTypes = ledger.GetPossibleAccountTypes();
            if (possibleAccountTypes.Count == 1) return possibleAccountTypes.ElementAt(0);
            if (preferenceAccountTypes.ContainsKey(ledger.GetPrintableName()))
            {
                var accountType = preferenceAccountTypes[ledger.GetPrintableName()];
                if (possibleAccountTypes.Contains(accountType)) return accountType;
                else return possibleAccountTypes.ElementAt(0);
            }
            else
            {
                return possibleAccountTypes.ElementAt(0);
            }
        }

        public string AccountName { get; set; }

        public string OverAllMessage { get; set; }

        public string AccountType { get; set; }

        public List<DisplayableAccountStatement> AccountStatements { get; set; }
    }

}