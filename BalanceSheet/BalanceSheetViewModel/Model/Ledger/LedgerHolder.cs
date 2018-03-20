using System;
using System.Collections.Generic;
using System.Linq;

namespace Nachiappan.BalanceSheetViewModel.Model.Ledger
{
    public class LedgerHolder
    {
        private readonly DateTime _openingDate;
        private readonly DateTime _closingDateTime;
        private Dictionary<string, RealLedger> realLeadgers = new Dictionary<string, RealLedger>();
        private Dictionary<string, NominalLedger> nominaLedgers = new Dictionary<string, NominalLedger>();
        private Dictionary<string, NominalLedger> doubleNominalLedgers = new Dictionary<string, NominalLedger>();

        public LedgerHolder(DateTime openingDate, DateTime closingDateTime)
        {
            _openingDate = openingDate;
            _closingDateTime = closingDateTime;
        }

        private bool IsRealLedgerName(string name)
        {
            return LedgerClassifer.IsRealLedger(name);
        }

        private bool IsNominalLedgerName(string name)
        {
            return LedgerClassifer.IsNominalLedger(name);
        }

        private bool IsDoubleNominalLedgerName(string name)
        {
            return LedgerClassifer.IsDoubleNominalLedger(name);
        }

        public List<ILedger> GetRealLedgers()
        {
            return realLeadgers.Select(x => x.Value).ToList<ILedger>();
        }

        public List<ILedger> GetAllLedgers()
        {
            var ledgers = realLeadgers.Select(x => x.Value).ToList<ILedger>();
            ledgers.AddRange(nominaLedgers.Select(x => x.Value).ToList());
            ledgers.AddRange(doubleNominalLedgers.Select(x => x.Value).ToList());
            return ledgers;
        }

        public void CloseNominalLedgers()
        {
            foreach (var doubleNominalLedger in doubleNominalLedgers)
            {
                CloseLedger(doubleNominalLedger.Value, doubleNominalLedger.Key);
            }
            foreach (var nominalLedger in nominaLedgers)
            {
                CloseLedger(nominalLedger.Value, nominalLedger.Key);
            }
        }

        private void CloseLedger(NominalLedger nominalLedger, string fullName)
        {
            var value = nominalLedger.GetLedgerValue();
            var baseLedgerName = LedgerClassifer.GetBasePartOfName(fullName);
            nominalLedger.PostTransaction(_closingDateTime, "Closing", value * -1);
            var baseLedger = GetLedger(baseLedgerName);
            baseLedger.PostTransaction(_closingDateTime, "Closing of " + LedgerClassifer.GetNominalPartOfName(fullName), value);
        }

        public ILedger GetLedger(string name)
        {
            if (IsRealLedgerName(name))
            {
                if (!realLeadgers.ContainsKey(name))
                {
                    realLeadgers.Add(name, new RealLedger(name, _openingDate, 0));
                }
                return realLeadgers[name];
            }

            if (IsNominalLedgerName(name))
            {
                if (!nominaLedgers.ContainsKey(name))
                {
                    nominaLedgers.Add(name, new NominalLedger(name));
                }
                return nominaLedgers[name];
            }

            if (IsDoubleNominalLedgerName(name))
            {
                if (!doubleNominalLedgers.ContainsKey(name))
                {
                    doubleNominalLedgers.Add(name, new NominalLedger(name));
                }
                return doubleNominalLedgers[name];
            }
            throw new Exception();
        }

        private string GetNominalPartOfName(string name)
        {
            return LedgerClassifer.GetNominalPartOfName(name);
        }

        private string GetBasePartOfName(string name)
        {
            return LedgerClassifer.GetBasePartOfName(name);
        }

        public void CreateRealLedger(string name, double value)
        {
            if (!IsRealLedgerName(name))throw new Exception();
            if(realLeadgers.ContainsKey(name)) throw new Exception();
            realLeadgers.Add(name, new RealLedger(name, _openingDate, value));
        }
    }
}