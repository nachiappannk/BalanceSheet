using System;
using System.Collections.Generic;

namespace Nachiappan.BalanceSheetViewModel
{
    public class DataStore
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        public bool IsPackageStored<T>(string packageName)
        {
            var isPackageStored = dictionary.ContainsKey(packageName);
            if (!isPackageStored) return false;
            var package = dictionary[packageName];
            if (package.GetType() == typeof(T)) return true;
            return false;
        }

        public T GetPackage<T>(string packageName)
        {
            if (IsPackageStored<T>(packageName)) return (T)dictionary[packageName];
            throw new Exception();
        }

        public void PutPackage<T>(T t, string packageName)
        {
            if (!dictionary.ContainsKey(packageName)) dictionary.Add(packageName, t);
            dictionary[packageName] = t;
        }
    }
}