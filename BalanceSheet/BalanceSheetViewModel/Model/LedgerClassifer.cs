using System;
using System.Text.RegularExpressions;

namespace Nachiappan.BalanceSheetViewModel.Model
{
    public static class LedgerClassifer
    {
        private const string AccountNamePattern = @"[a-zA-Z0-9\s!@#$%^&*]+";
        private const string SimpleAccountPattern = @"^("+AccountNamePattern+")$";
        private const string NominalAccountPattern = @"^(" + AccountNamePattern + ")/(" + AccountNamePattern + ")$";
        private const string DoubleNominalAccountPattern = @"^(" + AccountNamePattern + "/" + AccountNamePattern + ")/(" + AccountNamePattern + ")$";

        private static Regex _simpleAccountPatternRegex = new Regex(SimpleAccountPattern, RegexOptions.IgnoreCase);
        private static Regex _nominalAccountPatternRegex = new Regex(NominalAccountPattern, RegexOptions.IgnoreCase);
        private static Regex _doubleNominalAccountPatternRegex = new Regex(DoubleNominalAccountPattern, RegexOptions.IgnoreCase);

        public static bool IsRealLedger(string name)
        {
            return _simpleAccountPatternRegex.IsMatch(name);
        }

        public static bool IsNominalLedger(string name)
        {
            return _nominalAccountPatternRegex.IsMatch(name);
        }

        public static bool IsDoubleNominalLedger(string name)
        {
            return _doubleNominalAccountPatternRegex.IsMatch(name);
        }

        public static string GetNominalPartOfName(string name)
        {
            if (IsNominalLedger(name))
            {
                var match = _nominalAccountPatternRegex.Match(name);
                return match.Groups[2].Value;
            }
            else if(IsDoubleNominalLedger(name))
            {
                var match = _doubleNominalAccountPatternRegex.Match(name);
                return match.Groups[2].Value;
            }
            else
            {
                throw new Exception();
            }
        }

        public static string GetBasePartOfName(string name)
        {
            if (IsNominalLedger(name))
            {
                var match = _nominalAccountPatternRegex.Match(name);
                return match.Groups[1].Value;
            }
            else if (IsDoubleNominalLedger(name))
            {
                var match = _doubleNominalAccountPatternRegex.Match(name);
                return match.Groups[1].Value;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}