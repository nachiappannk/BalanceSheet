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

        private readonly Dictionary<string, Account> _accounts;
        private readonly Dictionary<string, int> _degreeOfNotionalness;
        private readonly Dictionary<string, string> _recipientAccounts;
        public GeneralAccount(DateTime openingDate, DateTime closingDateTime, 
            List<BalanceSheetStatement> previousBalanceSheetStatements, List<JournalStatement> journalStatements,
            List<AccountDefintionStatement> accountDefinitionStatements, Dictionary<string, string> printableNamesLookUp)
        {
            _openingDate = openingDate;
            _closingDateTime = closingDateTime;
            _journalStatements = journalStatements;
            _accountDefinitionStatements = accountDefinitionStatements;
            _recipientAccounts = _accountDefinitionStatements.ToDictionary(x => x.Account, x => x.RecipientAccount);
            _degreeOfNotionalness = new NotionalnessComputer().ComputerNotionalness(accountDefinitionStatements);

            var printableAccountName = 

            _accounts = _accountDefinitionStatements
                .Select(x => new Account(x.Account, x.AccountType, printableNamesLookUp))
                .ToDictionary(x => x.GetName(), x => x);

            PostOpeningStatements(previousBalanceSheetStatements);
            PostStatements(journalStatements);
            CloseAccounts();
        }
        
        private void PostOpeningStatements(List<BalanceSheetStatement> previousBalanceSheetStatements)
        {
            foreach (var balanceSheetStatement in previousBalanceSheetStatements)
            {
                var ledger = GetLedger(balanceSheetStatement.Account);
                ledger.PostStatement(_openingDate, "Opening Balance", balanceSheetStatement.Value);
            }
        }

        private void PostStatements(List<JournalStatement> journalStatements)
        {
            foreach (var journalStatement in journalStatements)
            {
                var ledger = GetLedger(journalStatement.Account);
                ledger.PostStatement(journalStatement.Date, journalStatement.Description, journalStatement.Value);
            }
        }

        private List<IAccount> GetRealAccounts()
        {
            return _accounts.Select(x => x.Value).ToList<IAccount>();
        }

        public List<IAccount> GetAllAccounts()
        {
            return _accounts.Select(x => x.Value).ToList<IAccount>();
        }

        private void CloseAccounts()
        {

            var degreeOfNotionalness = _degreeOfNotionalness.Select(x => x.Value).Max();
            for (int i = degreeOfNotionalness; i > 0; i--)
            {
                var accountsToBeClosed = _degreeOfNotionalness.Where(x => x.Value == i).Select(x => x.Key);
                foreach (var accountNameOfAccountToBeClosed in accountsToBeClosed)
                {
                    var accountToBeClosed = _accounts[accountNameOfAccountToBeClosed];
                    var accountNameOfRecipientAccount = _recipientAccounts[accountNameOfAccountToBeClosed];
                    var recipientAccount = _accounts[accountNameOfRecipientAccount];
                    var value = accountToBeClosed.GetAccountValue();
                    accountToBeClosed.PostStatement(_closingDateTime, 
                        "Closing and Transfer of balance to " + recipientAccount.GetPrintableName(), value * -1);
                    recipientAccount.PostStatement(_closingDateTime, "Transfer from "+accountToBeClosed.GetPrintableName(), value);
                }
            }
        }

        public List<BalanceSheetStatement> GetBalanceSheetStatements()
        {
            var ledgers = this.GetRealAccounts();
            return ledgers.Select(x => new BalanceSheetStatement()
            {
                Account = x.GetName(),
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

        

        private IAccount GetLedger(string name)
        {
            if (_accounts.ContainsKey(name))
            {
                return _accounts[name];
            }
            throw new Exception();
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