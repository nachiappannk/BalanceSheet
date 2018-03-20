using System;
using System.Collections.Generic;

namespace Nachiappan.BalanceSheetViewModel
{
    public class DataStore
    {
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public bool IsPackageStored<T>(string packageName)
        {
            var isPackageStored = _dictionary.ContainsKey(packageName);
            if (!isPackageStored) return false;
            var package = _dictionary[packageName];
            if (package.GetType() == typeof(T)) return true;
            return false;
        }

        public T GetPackage<T>(string packageName)
        {
            if (IsPackageStored<T>(packageName)) return (T)_dictionary[packageName];
            throw new Exception();
        }

        public void PutPackage<T>(T t, string packageName)
        {
            if (!_dictionary.ContainsKey(packageName)) _dictionary.Add(packageName, t);
            _dictionary[packageName] = t;
        }
    }

    // ReSharper disable once UnusedTypeParameter
    public class PackageDefinition<T>
    {
        public PackageDefinition(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }

    public static class DataStoreExtentions
    {
        public static bool IsPackageStored<T>(this DataStore dataStore, PackageDefinition<T> packageDefinition)
        {
            return dataStore.IsPackageStored<T>(packageDefinition.Name);
        }

        public static T GetPackage<T>(this DataStore dataStore, PackageDefinition<T> packageDefinition)
        {
            return dataStore.GetPackage<T>(packageDefinition.Name);
        }

        public static void PutPackage<T>(this DataStore dataStore, T t, PackageDefinition<T> packageDefinition)
        {
            dataStore.PutPackage(t, packageDefinition.Name);
        }
    }
}