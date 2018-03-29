using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.Statements;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel
{
    public class StatementVerifyingWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public List<DisplayableBalanceSheetStatement> PreviousBalanceSheetStatements { get; set; }
        public List<DisplayableBalanceSheetStatement> BalanceSheetStatements { get; set; }
        public List<DisplayableJournalStatement> JournalStatements { get; set; }
        public List<DisplayableAccountDefintionStatement> AccountDefinitionStatements { get; set; }
        public List<DisplayableTrialBalanceStatement> TrialBalanceStatements { get; set; }


        private string _selectedLedgerName;
        private readonly Dictionary<string, IAccount> _accounts;
        private SelectedAccountViewModel _selectedAccountViewModel;

        public StatementVerifyingWorkFlowStepViewModel(DataStore dataStore, Action goToPreviousStep, 
            Action goToNextStep)
        {

            FinancialStatementsComputer.ComputerFinanicalStatemments(dataStore);


            GoToPreviousCommand = new DelegateCommand(goToPreviousStep);
            GoToNextCommand = new DelegateCommand(goToNextStep);
            Name = "Verify Input/Output Statements";

            PreviousBalanceSheetStatements = GetBalanceSheetStatements(dataStore, WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition);
            BalanceSheetStatements = GetBalanceSheetStatements(dataStore, WorkFlowViewModel.BalanceSheetStatementsPackageDefinition);

            JournalStatements = GetInputJournalStatement(dataStore);
            TrialBalanceStatements = GetTrailBalanceStatements(dataStore);

            var accoutDifinitionStatements =
                dataStore.GetPackage(WorkFlowViewModel.InputAccountDefinitionPackageDefinition)
                    .MakeAccountPrintable(dataStore, 
                    WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition)
                .MakeRecipientAccountPrintable(dataStore,
                WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);
            AccountDefinitionStatements = accoutDifinitionStatements
                .Select(x => new DisplayableAccountDefintionStatement(x)).ToList();

            _accounts = CreateAccountDictionary(dataStore);

            LedgerNames = _accounts.Select(x => x.Value.GetPrintableName()).ToList();
            SelectedLedgerName = LedgerNames.ElementAt(0);

            
        }

        private static Dictionary<string, IAccount> CreateAccountDictionary(DataStore dataStore)
        {
            var allLedgers = dataStore.GetPackage(WorkFlowViewModel.AccountsPackageDefinition);
            var dictionary = allLedgers.ToDictionary(x => x.GetPrintableName(), x => x);
            return dictionary;
        }

        private List<DisplayableTrialBalanceStatement> GetTrailBalanceStatements(DataStore dataStore)
        {
            var trialBalanceStatements = dataStore
                .GetPackage(WorkFlowViewModel.TrialBalanceStatementsPackageDefinition)
                .MakeAccountPrintable(dataStore,
                    WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);


            var displayableTrialBalanceStatements = trialBalanceStatements
                .Select(x => new DisplayableTrialBalanceStatement(x)).ToList();
                

            displayableTrialBalanceStatements.Add(new DisplayableTrialBalanceStatement());
            var debitTotal = trialBalanceStatements.GetDebitTotal();
            var creditTotal = trialBalanceStatements.GetCreditTotal();

            displayableTrialBalanceStatements.Add(new DisplayableTrialBalanceStatement()
            {
                Account = "Total",
                Credit = creditTotal,
                Debit = debitTotal,
            });

            if (!(creditTotal - debitTotal).IsZero())
            {
                double? nullDouble = null;
                double? creditDifference = (creditTotal > debitTotal) ? creditTotal - debitTotal : nullDouble;
                double? debitDifference = (debitTotal > creditTotal) ? debitTotal - creditTotal : nullDouble;

                displayableTrialBalanceStatements.Add(new DisplayableTrialBalanceStatement()
                {

                    Account = "Difference",
                    Credit = creditDifference,
                    Debit = debitDifference
                });
            }

            

            return displayableTrialBalanceStatements;
        }

        private List<DisplayableJournalStatement> GetInputJournalStatement(DataStore dataStore)
        {
            var journalStatements = dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion)
                .MakeAccountPrintable(dataStore,
                    WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);

            var displayableJournalStatements = journalStatements.Select(x =>
                new DisplayableJournalStatement(x)).ToList();
            
            displayableJournalStatements.Add(new DisplayableJournalStatement());
            var creditTotal = journalStatements.GetCreditTotal();
            var debitTotal = journalStatements.GetDebitTotal();
            displayableJournalStatements.Add(new DisplayableJournalStatement()
            {
                Account = "Total",
                Credit = creditTotal,
                Debit = debitTotal,
            });

            if (!(creditTotal - debitTotal).IsZero())
            {
                if (creditTotal > debitTotal)
                {
                    displayableJournalStatements.Add(new DisplayableJournalStatement()
                    {
                        Account = "Difference",
                        Credit = creditTotal - debitTotal,
                    });
                }
                else
                {
                    displayableJournalStatements.Add(new DisplayableJournalStatement()
                    {
                        Account = "Difference",
                        Debit = debitTotal - creditTotal,
                    });
                }
            }



            return displayableJournalStatements;
        }

        private List<DisplayableBalanceSheetStatement> GetBalanceSheetStatements(DataStore dataStore, PackageDefinition<List<BalanceSheetStatement>> packageDefinition)
        {
            var statements = dataStore.GetPackage(packageDefinition)
                .MakeAccountPrintable(dataStore,
                WorkFlowViewModel.DisplayableAccountNamesDictionaryPackageDefinition);


            var displayableStatements = statements
                .Select(x => new DisplayableBalanceSheetStatement(x))
                .ToList();

            displayableStatements.Add(new DisplayableBalanceSheetStatement());
            var creditTotal = statements.GetCreditTotal();
            var debitTotal = statements.GetDebitTotal();
            var totalStatement = new DisplayableBalanceSheetStatement
            {
                Account = "Total",
                Credit = creditTotal,
                Debit = debitTotal
            };
            displayableStatements.Add(totalStatement);

            if (!(creditTotal - debitTotal).IsZero())
            {
                if (creditTotal > debitTotal)
                {
                    displayableStatements.Add(new DisplayableBalanceSheetStatement()
                    {
                        Account = "Difference",
                        Credit = creditTotal - debitTotal,
                    });
                }
                else
                {
                    displayableStatements.Add(new DisplayableBalanceSheetStatement()
                    {
                        Account = "Difference",
                        Debit = debitTotal - creditTotal,
                    });
                }
            }

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
                    SelectedAccountViewModel = new SelectedAccountViewModel(_accounts, value);
                }
                FirePropertyChanged();
            }
        }

        
    }

    public class SelectedAccountViewModel
    {
        public SelectedAccountViewModel(IDictionary<string, IAccount> accounts, string selectedAccount)
        {
            var account = accounts[selectedAccount];
            var accountType = account.GetAccountType();
            var statements = account.GetAccountStatements();
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
                case Model.Statements.AccountType.Notional:
                    return "Notional Account";
                case Model.Statements.AccountType.Equity:
                    return "The total equity contribution from this account is " + account.GetAccountValue().ToString("N2");
                case Model.Statements.AccountType.Asset:
                    return "The investment is " + (account.GetAccountValue() * -1).ToString("N2"); ;
                case Model.Statements.AccountType.Liability:
                default:
                    return "The borrowing is " + account.GetAccountValue().ToString("N2"); ;

            }
        }
        
        public string AccountName { get; set; }

        public string OverAllMessage { get; set; }

        public string AccountType { get; set; }

        public List<DisplayableAccountStatement> AccountStatements { get; set; }
    }

}