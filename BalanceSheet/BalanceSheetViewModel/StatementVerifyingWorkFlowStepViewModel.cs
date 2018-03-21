using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.Statements;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public class StatementVerifyingWorkFlowStepViewModel : WorkFlowStepViewModel
    {        
        private string _selectedLedgerName;
        private Dictionary<string, IAccount> _ledgers;
        private List<DisplayableLedgerStatement> _ledgerStatements;
        private string _ledgerType;
        private Dictionary<string, AccountType> _ledgerTypes;

        public StatementVerifyingWorkFlowStepViewModel(DataStore dataStore, Action goToProcessingStep, 
            Action goToPrintStatementWorkFlowStep)
        {

            _ledgerTypes = dataStore.GetPackage(WorkFlowViewModel.AccountNameToTypeMapPackageDefinition);

            GoToPreviousCommand = new DelegateCommand(goToProcessingStep);
            GoToNextCommand = new DelegateCommand(goToPrintStatementWorkFlowStep);
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
                .Select(x => new DisplayableTrialBalanceStatement()
                {
                    Account = x.Account,
                    Tag = x.Tag,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                }).ToList();
        }

        private List<DisplayableTrimmedJournalStatement> GetTrimmedStatements(DataStore dataStore)
        {
            var journalStatements = dataStore.GetPackage(WorkFlowViewModel.TrimmedJournalStatementsPackageDefintion);
            return journalStatements.Select(x =>
                new DisplayableTrimmedJournalStatement()   
                {
                    Description = x.Account,
                    Date = x.Date,
                    DetailedDescription = x.Description,
                    Tag = x.Tag,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                    Reason = x.Reason,
                }).ToList();
        }

        private List<DisplayableJournalStatement> GetInputJournalStatement(DataStore dataStore)
        {
            var journalStatements = dataStore.GetPackage(WorkFlowViewModel.InputJournalStatementsPackageDefintion);
            return journalStatements.Select(x =>
                new DisplayableJournalStatement()
                {
                    Description = x.Account,
                    Date = x.Date,
                    DetailedDescription = x.Description,
                    Tag = x.Tag,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                }).ToList();
        }

        private List<DisplayableStatement> GetBalanceSheetStatements(DataStore dataStore, PackageDefinition<List<BalanceSheetStatement>> packageDefinition)
        {
            var statements = dataStore.GetPackage(packageDefinition);
            var displayableStatements = statements
                .Select(x => new DisplayableStatement()
                {
                    Description = x.Account,
                    Credit = HasValueExtentions.GetCreditValueOrNull(x),
                    Debit = HasValueExtentions.GetDebitValueOrNull(x),
                })
                .ToList();
            return displayableStatements;
        }

        public List<DisplayableStatement>   PreviousBalanceSheetStatements { get; set; }

        public List<DisplayableTrimmedBalanceSheetStatement> TrimmedBalanceSheetStatements { get; set; }

        public List<DisplayableStatement> BalanceSheetStatements { get; set; }

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

        public List<DisplayableLedgerStatement> LedgerStatements
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

                    var ledgerType = Model.Account.AccountType.Asset;
                    if (_ledgerTypes.ContainsKey(value))
                    {
                        ledgerType = _ledgerTypes[value];
                    }
                    
                    var statements = ledger.GetAccountStatements(ledgerType);

                    LedgerStatements = statements.Select(x => new DisplayableLedgerStatement()
                    {
                        SerialNumber = x.SerialNumber,
                        Description = x.Description,
                        Date = x.Date,
                        Credit = x.GetCreditValueOrNull(),
                        Debit = x.GetDebitValueOrNull(),
                    }).ToList();
                    LedgerType = ledger.GetAccountType();
                }

                FirePropertyChanged();
            }
        }
    }


    public class DisplayableLedgerStatement
    {
        public int SerialNumber { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double? Credit { get; set; }
        public double? Debit { get; set; }
    }

    public class DisplayableTrialBalanceStatement
    {
        [DisplayName("Account")]
        public string Account { get; set; }

        [DisplayName("Tag")]
        public string Tag { get; set; }

        [DisplayName("Credit")]
        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        public double? Credit { get; set; }

        [DisplayName("Debit")]
        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        public double? Debit { get; set; }
    }


    public class DisplayableStatement
    {
        [DisplayName("Account")]
        public string Description { get; set; }

        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Credit")]
        public double? Credit { get; set; }


        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Debit")]
        public double? Debit { get; set; }
    }

    public class DisplayableJournalStatement
    {

        [DisplayName("Date")]
        [DisplayFormat(DataFormatString = CommonDefinition.DateDisplayFormat)]
        public DateTime Date { get; set; }

        [DisplayName("Account")]
        public string Description { get; set; }

        [DisplayName("Tag")]
        public string Tag { get; set; }

        [DisplayName("Description")]
        public string DetailedDescription { get; set; }

        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Credit")]
        public double? Credit { get; set; }


        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Debit")]
        public double? Debit { get; set; }
        
    }

    public class DisplayableTrimmedJournalStatement
    {

        [DisplayName("Date")]
        [DisplayFormat(DataFormatString = CommonDefinition.DateDisplayFormat)]
        public DateTime Date { get; set; }

        [DisplayName("Account")]
        public string Description { get; set; }

        [DisplayName("Tag")]
        public string Tag { get; set; }

        [DisplayName("Description")]
        public string DetailedDescription { get; set; }

        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Credit")]
        public double? Credit { get; set; }


        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Debit")]
        public double? Debit { get; set; }

        public string Reason { get; set; }

    }


    public class DisplayableTrimmedBalanceSheetStatement
    {
        public DisplayableTrimmedBalanceSheetStatement(TrimmedBalanceSheetStatement statement)
        {
            Account = statement.Account;
            Credit = statement.GetCreditValueOrNull();
            Debit = statement.GetDebitValueOrNull();
            Reason = statement.Reason;
        }

        [DisplayName("Account")]
        public string Account { get; set; }

        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Credit")]
        public double? Credit { get; set; }

        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Debit")]
        public double? Debit { get; set; }

        [DisplayName("Reason")]
        public string Reason { get; set; }
    }


    public static class CommonDefinition
    {
        public const string DateDisplayFormat = "dd-MMM-yyyy";
        public const string ValueDisplayFormat = "N2";
        public const string QuantityDisplayFormat = "#.###";
    }
}