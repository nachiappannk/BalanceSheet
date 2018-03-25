using System;
using System.Text.RegularExpressions;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public static class AccountClassifer
    {
        private const string AccountNamePattern = @"[a-zA-Z0-9\s!@#$%^&*]+";
        private const string RealAccountPattern = @"^([a-zA-Z0-9\s!@#$%^&*]+)$";
        private const string NominalAccountPattern = @"^(" + AccountNamePattern + ")/(" + AccountNamePattern + ")$";
        private const string DoubleNominalAccountPattern = @"^(" + AccountNamePattern + "/" + AccountNamePattern + ")/(" + AccountNamePattern + ")$";

        private static Regex _accountPatternRegex = new Regex(RealAccountPattern, RegexOptions.IgnoreCase);
        private static Regex _nominalAccountPatternRegex = new Regex(NominalAccountPattern, RegexOptions.IgnoreCase);
        private static Regex _doubleNominalAccountPatternRegex = new Regex(DoubleNominalAccountPattern, RegexOptions.IgnoreCase);

        public static bool IsAccountNameValid(string name)
        {
            return _accountPatternRegex.IsMatch(name);
        }

        public static bool IsNominalLedger(string name)
        {
            return _nominalAccountPatternRegex.IsMatch(name);
        }

        public static bool IsDoubleNominalLedger(string name)
        {
            return _doubleNominalAccountPatternRegex.IsMatch(name);
        }
    }
}