using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.StatementDisplayingViewModel
{
    public class DisplayableTrialBalanceStatement
    {
        public DisplayableTrialBalanceStatement(TrialBalanceStatement x)
        {
            Account = x.Account;
            Tag = x.Tag;
            Credit = x.GetCreditValueOrNull();
            Debit = x.GetDebitValueOrNull();
        }


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
}