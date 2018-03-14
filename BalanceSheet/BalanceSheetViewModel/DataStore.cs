using System;
using System.Collections.Generic;

namespace Nachiappan.BalanceSheetViewModel
{
    public class DataStore
    {
        Dictionary<Type, object> dictionary = new Dictionary<Type, object>();

        public bool IsPackageStored<T>()
        {
            return dictionary.ContainsKey(typeof(T));
        }

        public T GetPackage<T>()
        {
            if (IsPackageStored<T>()) return (T)dictionary[typeof(T)];
            else throw new Exception();
        }

        public void PutPackage<T>(T t)
        {
            if (!IsPackageStored<T>()) dictionary.Add(typeof(T), t);
            dictionary[typeof(T)] = t;
        }
    }
}