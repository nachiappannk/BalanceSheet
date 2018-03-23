using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.StatementVerifyingWorkFlowStepViewModel
{
    public class DisplayableJournalStatement
    {

        public DisplayableJournalStatement(JournalStatement x)
        {
            Account = x.Account;
            Date = x.Date;
            Description = x.Description;
            Tag = x.Tag;
            Credit = x.GetCreditValueOrNull();
            Debit = x.GetDebitValueOrNull();
        }


        [DisplayName("Date")]
        [DisplayFormat(DataFormatString = CommonDefinition.DateDisplayFormat)]
        public DateTime Date { get; set; }

        [DisplayName("Account")]
        public string Account { get; set; }

        [DisplayName("Tag")]
        public string Tag { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Credit")]
        public double? Credit { get; set; }


        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Debit")]
        public double? Debit { get; set; }
        
    }
}