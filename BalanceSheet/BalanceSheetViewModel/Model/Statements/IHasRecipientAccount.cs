using System.Collections.Generic;
using System.Linq;

namespace Nachiappan.BalanceSheetViewModel.Model.Statements
{
    public interface IHasRecipientAccount
    {
        string RecipientAccount { get; set; }
    }

    public static class HasRecipientAccountExtentions
    {
        public static List<T> MakeRecipientAccountPrintable<T>(this IEnumerable<T> statements, Dictionary<string, string> nameLookUp) 
            where T : ICanClone<T>, IHasRecipientAccount
        {
            var results = statements.Select(x => x.Clone()).ToList();
            results.ForEach(x =>
            {
                if (x.RecipientAccount != null)
                {
                    var recipientAccount = x.RecipientAccount;
                    if (nameLookUp.ContainsKey(recipientAccount))
                    {
                        x.RecipientAccount = nameLookUp[recipientAccount];
                    }
                }
            });
            return results;
        }

        public static List<T> MakeRecipientAccountPrintable<T>(this IEnumerable<T> statements, DataStore dataStore,
            PackageDefinition<Dictionary<string, string>> nameLookUpPackageDefinition) where T : ICanClone<T>, IHasRecipientAccount
        {
            var nameLookUp = dataStore.GetPackage(nameLookUpPackageDefinition);
            return statements.MakeRecipientAccountPrintable(nameLookUp);
        }
    }
}