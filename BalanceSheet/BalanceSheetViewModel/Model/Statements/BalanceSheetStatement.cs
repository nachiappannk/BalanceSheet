﻿namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public class BalanceSheetStatement : IHasValue
    {
        public string Account { get; set; }
        public double Value { get; set; }
    }
}