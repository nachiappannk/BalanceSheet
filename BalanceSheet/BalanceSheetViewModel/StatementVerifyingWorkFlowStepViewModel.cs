using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public class StatementVerifyingWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private string _selectedLedgerName;
        private Dictionary<string, ILedger> _ledgers;
        private List<DisplayableLedgerStatement> _ledgerStatements;
        private string _ledgerType;

        public StatementVerifyingWorkFlowStepViewModel(DataStore dataStore, Action goToProcessingStep, 
            Action goToPrintStatementWorkFlowStep)
        {
            GoToPreviousCommand = new DelegateCommand(goToProcessingStep);
            GoToNextCommand = new DelegateCommand(goToPrintStatementWorkFlowStep);
            Name = "Verify Input/Output Statements";
            PreviousBalanceSheetStatements = GetBalanceSheetStatements(dataStore, WorkFlowViewModel.PreviousBalanceSheetPacakge);
            BalanceSheetStatements = GetBalanceSheetStatements(dataStore, WorkFlowViewModel.BalanceSheetPackage);
            JournalStatements = GetStatements(dataStore, WorkFlowViewModel.InputJournalPackage);
            TrimmedJournalStatements = GetStatements(dataStore, WorkFlowViewModel.TrimmedJournalPackage);
            SetTrailBalanceStatements(dataStore);
            _ledgers = dataStore.GetPackage<Dictionary<string, ILedger>>(WorkFlowViewModel.LedgersPackage);
            LedgerNames = _ledgers.Select(x => x.Key).ToList();
            SelectedLedgerName = LedgerNames.ElementAt(0);


        }

        private void SetTrailBalanceStatements(DataStore dataStore)
        {
            TrialBalanceStatements = dataStore
                .GetPackage<List<TrialBalanceStatement>>(WorkFlowViewModel.TrialBalancePackage)
                .Select(x => new DisplayableTrialBalanceStatement()
                {
                    Account = x.Account,
                    Tag = x.Tag,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                }).ToList();
        }

        private List<DisplayableJournalStatement> GetStatements(DataStore dataStore, string packageName)
        {
            var journalStatements = dataStore.GetPackage<List<JournalStatement>>(packageName);
            return journalStatements.Select(x =>
                new DisplayableJournalStatement()   
                {
                    Description = x.Description,
                    Date = x.Date,
                    DetailedDescription = x.DetailedDescription,
                    Tag = x.Tag,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                }).ToList();
        }

        private List<DisplayableStatement> GetBalanceSheetStatements(DataStore dataStore, string packageName)
        {
            var statements = dataStore.GetPackage<List<Statement>>(packageName);
            var displayableStatements = statements
                .Select(x => new DisplayableStatement()
                {
                    Description = x.Description,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                })
                .ToList();
            return displayableStatements;
        }

        public List<DisplayableStatement>   PreviousBalanceSheetStatements { get; set; }

        public List<DisplayableStatement> BalanceSheetStatements { get; set; }

        public List<DisplayableJournalStatement> JournalStatements { get; set; }

        public List<DisplayableJournalStatement> TrimmedJournalStatements { get; set; }

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
                    var statements = ledger.GetLedgerStatements();
                    LedgerStatements = statements.Select(x => new DisplayableLedgerStatement()
                    {
                        SerialNumber = x.SerialNumber,
                        Description = x.Description,
                        Date = x.Date,
                        Credit = x.GetCreditValueOrNull(),
                        Debit = x.GetDebitValueOrNull(),
                    }).ToList();
                    LedgerType = ledger.GetLedgerType();
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


    public static class CommonDefinition
    {
        public const string DateDisplayFormat = "dd-MMM-yyyy";
        public const string ValueDisplayFormat = "N2";
        public const string QuantityDisplayFormat = "#.###";
    }
}