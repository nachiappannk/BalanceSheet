using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.StatementVerifyingWorkFlowStepViewModel
{
    public class DisplayableAccountStatement
    {
        public DisplayableAccountStatement(AccountStatement x)
        {
            SerialNumber = x.SerialNumber;
            Description = x.Description;
            Date = x.Date;
            Credit = x.GetCreditValueOrNull();
            Debit = x.GetDebitValueOrNull();
        }

        [DisplayName("S.No.")]
        public int SerialNumber { get; set; }

        [DisplayName("Date")]
        [DisplayFormat(DataFormatString = CommonDefinition.DateDisplayFormat)]
        public DateTime Date { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Credit")]
        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        public double? Credit { get; set; }

        [DisplayName("Debit")]
        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        public double? Debit { get; set; }
    }
}