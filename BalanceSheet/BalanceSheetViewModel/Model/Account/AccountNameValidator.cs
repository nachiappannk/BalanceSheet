using System;
using System.Text.RegularExpressions;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public static class AccountNameValidator
    {
        private const string RealAccountPattern = @"^([a-zA-Z0-9\s!@#$%^&*]+)$";

        private static Regex _accountPatternRegex = new Regex(RealAccountPattern, RegexOptions.IgnoreCase);

        public static bool IsAccountNameValid(string name)
        {
            return _accountPatternRegex.IsMatch(name);
        }
    }
}