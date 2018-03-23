using System;
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
        private string _selectedLedgerName;
        private Dictionary<string, IAccount> _ledgers;
        private List<DisplayableAccountStatement> _ledgerStatements;
        private string _ledgerType;
        private Dictionary<string, AccountType> _ledgerTypes;

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
            SetTrailBalanceStatements(dataStore);
            var allLedgers = dataStore.GetPackage(WorkFlowViewModel.AccountsPackageDefinition);
            _ledgers = allLedgers.ToDictionary(x => x.GetPrintableName(), x => x);
            LedgerNames = _ledgers.Select(x => x.Key).ToList();
            SelectedLedgerName = LedgerNames.ElementAt(0);

            TrimmedBalanceSheetStatements = dataStore
                    .GetPackage(WorkFlowViewModel.TrimmedPreviousBalanceSheetStatements)
                    .Select(x => new DisplayableTrimmedBalanceSheetStatement(x)).ToList();
        }

        private void SetTrailBalanceStatements(DataStore dataStore)
        {
            TrialBalanceStatements = dataStore
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

        public List<DisplayableBalanceSheetStatement>   PreviousBalanceSheetStatements { get; set; }

        public List<DisplayableTrimmedBalanceSheetStatement> TrimmedBalanceSheetStatements { get; set; }

        public List<DisplayableBalanceSheetStatement> BalanceSheetStatements { get; set; }

        public List<DisplayableJournalStatement> JournalStatements { get; set; }

        public List<DisplayableTrimmedJournalStatement> TrimmedJournalStatements { get; set; }

        public List<DisplayableTrialBalanceStatement> TrialBalanceStatements { get; set; }


        public string LedgerType
        {
            get { return _ledgerType; }
            set
            {
                _ledgerType = value;
                FirePropertyChanged();
            }
        }

        public List<DisplayableAccountStatement> LedgerStatements
        {
            get { return _ledgerStatements; }
            set
            {
                _ledgerStatements = value;
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
                if (_ledgers.ContainsKey(value))
                {
                    var ledger = _ledgers[value];

                    var ledgerType = AccountType.Asset;
                    if (_ledgerTypes.ContainsKey(value))
                    {
                        ledgerType = _ledgerTypes[value];
                    }
                    
                    var statements = ledger.GetAccountStatements(ledgerType);

                    LedgerStatements = statements.Select(x => new DisplayableAccountStatement(x)).ToList();

                    var accountType = GetAccountType(ledger);


                    LedgerType = accountType.ToString();
                }

                FirePropertyChanged();
            }
        }

        private AccountType GetAccountType(IAccount ledger)
        {
            var accountTypes = ledger.GetPossibleAccountTypes();
            if (accountTypes.Count == 1) return accountTypes.ElementAt(0);
            if (_ledgerTypes.ContainsKey(ledger.GetPrintableName()))
            {
                var accountType = _ledgerTypes[ledger.GetPrintableName()];
                if (accountTypes.Contains(accountType)) return accountType;
                else return accountTypes.ElementAt(0);
            }
            else
            {
                return accountTypes.ElementAt(0);
            }
        }
    }
}