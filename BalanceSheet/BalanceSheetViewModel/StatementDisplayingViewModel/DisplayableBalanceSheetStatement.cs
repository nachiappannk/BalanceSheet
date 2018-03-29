using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel
{
    public class DisplayableBalanceSheetStatement
    {

        public DisplayableBalanceSheetStatement()
        {
            Account = null;
            Credit = null;
            Debit = null;
        }

        public DisplayableBalanceSheetStatement(BalanceSheetStatement x)
        {

            Account = x.Account;
            Credit = x.GetCreditValueOrNull();
            Debit = x.GetDebitValueOrNull();
        }

        [DisplayName("Account")]
        public string Account { get; set; }

        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Credit")]
        public double? Credit { get; set; }


        [DisplayFormat(DataFormatString = CommonDefinition.ValueDisplayFormat)]
        [DisplayName("Debit")]
        public double? Debit { get; set; }
    }
}