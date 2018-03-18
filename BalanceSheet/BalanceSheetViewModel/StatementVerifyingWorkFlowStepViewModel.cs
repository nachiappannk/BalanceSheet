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
        public StatementVerifyingWorkFlowStepViewModel(DataStore dataStore, Action goToProcessingStep, 
            Action goToPrintStatementWorkFlowStep)
        {
            GoToPreviousCommand = new DelegateCommand(goToProcessingStep);
            GoToNextCommand = new DelegateCommand(goToPrintStatementWorkFlowStep);
            Name = "Verify Input/Output Statements";

            SetBalanceSheetStatements(dataStore);
            JournalStatements = GetStatements(dataStore, WorkFlowViewModel.InputJournalPackage);
            TrimmedJournalStatements = GetStatements(dataStore, WorkFlowViewModel.TrimmedJournalPackage);


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

        private void SetBalanceSheetStatements(DataStore dataStore)
        {
            var statements = dataStore.GetPackage<List<Statement>>(WorkFlowViewModel.PreviousBalanceSheetPacakge);
            var displayableStatements = statements
                .Select(x => new DisplayableStatement()
                {
                    Description = x.Description,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                })
                .ToList();
            BalanceSheetStatements = displayableStatements;
        }

        public List<DisplayableStatement>   BalanceSheetStatements { get; set; }

        public List<DisplayableJournalStatement> JournalStatements { get; set; }

        public List<DisplayableJournalStatement> TrimmedJournalStatements { get; set; }

        public List<DisplayableTrialBalanceStatement> TrialBalanceStatements { get; set; }
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