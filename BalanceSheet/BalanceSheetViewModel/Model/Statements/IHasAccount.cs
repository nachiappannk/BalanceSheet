using System.Collections.Generic;
using System.Linq;

namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public interface IHasAccount
    {
        string Account { get; set; }
    }

    public static class HasAccountExtentions
    {
        public static List<T> MakeAccountPrintable<T>(this IEnumerable<T> statements, Dictionary<string, string> nameLookUp) where T : ICanClone<T>, IHasAccount
        {
            var results = statements.Select(x => x.Clone()).ToList();
            results.ForEach(x =>
            {
                var account = x.Account;
                if (nameLookUp.ContainsKey(account))
                {
                    x.Account = nameLookUp[account];
                }
            });
            return results;
        }

        public static List<T> MakeAccountPrintable<T>(this IEnumerable<T> statements, DataStore dataStore, 
            PackageDefinition<Dictionary<string,string>> nameLookUpPackageDefinition) where T : ICanClone<T>, IHasAccount
        {
            var nameLookUp = dataStore.GetPackage(nameLookUpPackageDefinition);
            return statements.MakeAccountPrintable(nameLookUp);
        }
    }
}