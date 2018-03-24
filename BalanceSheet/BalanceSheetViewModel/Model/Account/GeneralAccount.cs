using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.Account
{
    public class GeneralAccount
    {
        private readonly DateTime _openingDate;
        private readonly DateTime _closingDateTime;
        private readonly List<JournalStatement> _journalStatements;
        private readonly List<AccountDefintionStatement> _accountDefinitionStatements;

        private readonly Dictionary<string, int> _degreeOfNotionalnessForAccounts;
        private readonly Dictionary<string, RealAccount> _realAccounts = new Dictionary<string, RealAccount>();
        private readonly Dictionary<string, NominalAccount> _nominaAccounts = new Dictionary<string, NominalAccount>();
        private readonly Dictionary<string, NominalAccount> _doubleNominalAccounts = new Dictionary<string, NominalAccount>();



        public GeneralAccount(DateTime openingDate, DateTime closingDateTime, 
            List<BalanceSheetStatement> previousBalanceSheetStatements, List<JournalStatement> journalStatements,
            List<AccountDefintionStatement> accountDefinitionStatements)
        {
            _openingDate = openingDate;
            _closingDateTime = closingDateTime;
            _journalStatements = journalStatements;
            _accountDefinitionStatements = accountDefinitionStatements;


            _degreeOfNotionalnessForAccounts = GetDegreeOfNotionalnessOfAccounts(accountDefinitionStatements);

            OpenAccounts(previousBalanceSheetStatements);
            PostStatements(journalStatements);
            CloseAccounts();
        }

        private Dictionary<string, int> GetDegreeOfNotionalnessOfAccounts(List<AccountDefintionStatement> accountDefinitionStatements)
        {
            return new NotionalnessComputer().ComputerNotionalness(accountDefinitionStatements);
        }
        
        private void OpenAccounts(List<BalanceSheetStatement> previousBalanceSheetStatements)
        {
            foreach (var balanceSheetStatement in previousBalanceSheetStatements)
            {
                CreateRealLedger(balanceSheetStatement.Account, balanceSheetStatement.Value);
            }
        }

        private void PostStatements(List<JournalStatement> journalStatements)
        {
            foreach (var journalStatement in journalStatements)
            {
                var ledger = this.GetLedger(journalStatement.Account);
                ledger.PostStatement(journalStatement.Date, journalStatement.Description, journalStatement.Value);
            }
        }


        private bool IsRealLedgerName(string name)
        {
            var def = _accountDefinitionStatements.FirstOrDefault(x => x.Account == name);
            if (def == null) return true;
            if (def.AccountType == AccountType.Asset) return true;
            if (def.AccountType == AccountType.Equity) return true;
            if (def.AccountType == AccountType.Liability) return true;
            return false;
        }

        private bool IsNominalLedgerName(string name)
        {
            return !IsRealLedgerName(name);
        }

        private bool IsDoubleNominalLedgerName(string name)
        {
            return false;
        }

        private List<IAccount> GetRealAccounts()
        {
            return _realAccounts.Select(x => x.Value).ToList<IAccount>();
        }

        public List<IAccount> GetAllAccounts()
        {
            var ledgers = _realAccounts.Select(x => x.Value).ToList<IAccount>();
            ledgers.AddRange(_nominaAccounts.Select(x => x.Value).ToList());
            ledgers.AddRange(_doubleNominalAccounts.Select(x => x.Value).ToList());
            return ledgers;
        }

        private void CloseAccounts()
        {
            foreach (var doubleNominalLedger in _doubleNominalAccounts)
            {
                CloseLedger(doubleNominalLedger.Value, doubleNominalLedger.Key);
            }
            foreach (var nominalLedger in _nominaAccounts)
            {
                //CloseLedger(nominalLedger.Value, nominalLedger.Key);
            }
        }

        public List<BalanceSheetStatement> GetBalanceSheetStatements()
        {
            var ledgers = this.GetRealAccounts();
            return ledgers.Select(x => new BalanceSheetStatement()
            {
                Account = x.GetPrintableName(),
                Value = x.GetAccountValue(),
            }).Where(x => !x.Value.IsZero()).ToList();
        }

        public List<TrialBalanceStatement> GetTrialBalanceStatements()
        {
            var groupedStatements = _journalStatements.GroupBy(x => x.Account + "<1234#1234>" + x.Tag).ToList();
            var trialBalanseStatements = groupedStatements.Select(x =>
            {
                return new TrialBalanceStatement()
                {
                    Account = x.ElementAt(0).Account,
                    Tag = x.ElementAt(0).Tag,
                    Value = x.Sum(y => y.Value),
                };
            }).ToList();
            return trialBalanseStatements;
        }

        private void CloseLedger(NominalAccount nominalAccount, string fullName)
        {
            var value = nominalAccount.GetAccountValue();
            var baseLedgerName = _accountDefinitionStatements.Where(x => x.Account == fullName).FirstOrDefault()
                .RecipientAccount;
            nominalAccount.PostStatement(_closingDateTime, "Closing", value * -1);
            var baseLedger = GetLedger(baseLedgerName);
            baseLedger.PostStatement(_closingDateTime, "Closing of " + AccountClassifer.GetNominalPartOfName(fullName), value);
        }

        private IAccount GetLedger(string name)
        {
            if (IsRealLedgerName(name))
            {
                if (!_realAccounts.ContainsKey(name))
                {
                    _realAccounts.Add(name, new RealAccount(name, _openingDate, 0));
                }
                return _realAccounts[name];
            }

            if (IsNominalLedgerName(name))
            {
                if (!_nominaAccounts.ContainsKey(name))
                {
                    _nominaAccounts.Add(name, new NominalAccount(name));
                }
                return _nominaAccounts[name];
            }

            if (IsDoubleNominalLedgerName(name))
            {
                if (!_doubleNominalAccounts.ContainsKey(name))
                {
                    _doubleNominalAccounts.Add(name, new NominalAccount(name));
                }
                return _doubleNominalAccounts[name];
            }
            throw new Exception();
        }

        private void CreateRealLedger(string name, double value)
        {
            if (!IsRealLedgerName(name))throw new Exception();
            if(_realAccounts.ContainsKey(name)) throw new Exception();
            _realAccounts.Add(name, new RealAccount(name, _openingDate, value));
        }
    }

    public class NotionalnessComputer
    {
        private Dictionary<string, int> _notianalnessDictionary = new Dictionary<string, int>();

        private Dictionary<string, string> _recipientAccounts = new Dictionary<string, string>();
        public Dictionary<string,int> ComputerNotionalness(List<AccountDefintionStatement> accountDefintionStatements)
        {
            _recipientAccounts = accountDefintionStatements.ToDictionary(x => x.Account, x => x.RecipientAccount);

            foreach (var accountDefintionStatement in accountDefintionStatements)
            {
                int degreeOfNotionalness = GetDegreeOfNotionalNess(accountDefintionStatement.Account);
            }
            return _notianalnessDictionary;
        }

        private int GetDegreeOfNotionalNess(string account)
        {
            if (_notianalnessDictionary.ContainsKey(account)) return _notianalnessDictionary[account];
            var recipientAccount = _recipientAccounts[account];
            if (recipientAccount == String.Empty)
            {
                _notianalnessDictionary.Add(account,0);
                return 0;
            }
            else
            {
                var notionalnessOfRecipientAccount = GetDegreeOfNotionalNess(recipientAccount);
                var notionalness = notionalnessOfRecipientAccount + 1;
                _notianalnessDictionary.Add(account, notionalness);
                return notionalness;
            }
        }
    }

}