using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel
{
    public class DisplayableTrimmedBalanceSheetStatement
    {
        public DisplayableTrimmedBalanceSheetStatement(CorrectedBalanceSheetStatement statement)
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
}