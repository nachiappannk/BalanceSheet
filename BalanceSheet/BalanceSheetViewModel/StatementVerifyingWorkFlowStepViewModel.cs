﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
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

            

            var statements = dataStore.GetPackage<List<Statement>>();
            var displayableStatements = statements
                .Select(x => new DisplayableStatement()
                {
                    Description = x.Description,
                    Credit = x.GetCreditValueOrNull(),
                    Debit = x.GetDebitValueOrNull(),
                })
                .ToList();
            BalanceSheetStatements = displayableStatements;

            var journalStatements = dataStore.GetPackage<List<JournalStatement>>();


            JournalStatements = journalStatements.Select(x =>
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

        public List<DisplayableStatement>   BalanceSheetStatements { get; set; }

        public List<DisplayableJournalStatement> JournalStatements { get; set; }
    }



    public class DisplayableStatement
    {
        [DisplayName("Account")]
        public string Description { get; set; }


        [DisplayName("Credit")]
        public double? Credit { get; set; }

        [DisplayName("Debit")]
        public double? Debit { get; set; }
    }

    public class DisplayableJournalStatement
    {
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public double? Credit { get; set; }
        public double? Debit { get; set; }
        public string DetailedDescription { get; set; }
        public string Tag { get; set; }
    }


    public static class CommonDefinition
    {
        public const string DateDisplayFormat = "dd-MMM-yyyy";
        public const string ValueDisplayFormat = "N2";
        public const string QuantityDisplayFormat = "#.###";
    }
}