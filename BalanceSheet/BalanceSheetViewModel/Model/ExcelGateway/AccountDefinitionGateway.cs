using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Excel;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.ExcelGateway
{
    public class AccountDefinitionGateway
    {
        private readonly string _inputFile;

        private List<string> _headings = new List<string>()
        {
            "Type", "Account",  "Recipient Account"
        };

        const int Type = 0;
        const int Account = 1;
        const int RecipientAccount = 2;

        public AccountDefinitionGateway(string inputFile)
        {
            _inputFile = inputFile;
        }

        public void WirteAccountDefinitions(IList<AccountDefintionStatement> accountDefintionStatements)
        {
            using (ExcelSheetWriter writer = new ExcelSheetWriter(_inputFile, "AccountDefinition"))
            {
                int index = 0;

                writer.Write(index++, _headings.ToArray());
                writer.SetColumnsWidth(12, 35, 35);
                writer.ApplyHeadingFormat(_headings.Count);
                writer.WriteList(index, accountDefintionStatements.ToList(),
                    (j, rowIndex) => new object[]
                    {
                        j.AccountType.ToString(),
                        j.Account,
                        j.RecipientAccount,
                    });

            }
        }


        public List<AccountDefintionStatement> GetAccountDefinitionStatements(ILogger logger, string sheetName)
        {

            using (ExcelReader reader = new ExcelReader(_inputFile, sheetName, logger))
            {

                SheetHeadingVerifier.VerifyHeadingNames(logger, reader, _headings);

                var accountDefintionStatements = reader.ReadAllLines(1, (r) =>
                {
                    var isRecipientAccountAvailable = r.IsValueAvailable(RecipientAccount);
                    var accountTypeString = r.ReadString(Type);
                    if (!Enum.TryParse<AccountType>(accountTypeString, true, out var accountType))
                    {
                        accountType = AccountType.Equity;
                        logger.Log(MessageType.Warning, $"In file{r.FileName}, ",
                            $"in sheet{r.SheetName}, ",
                            $"in line no. {r.LineNumber}, ",
                            "could not read the account type. Taken it as Equity");
                    }

                    
                    return new AccountDefintionStatement()
                    {
                        AccountType = accountType,
                        Account = r.ReadString(Account),
                        RecipientAccount = isRecipientAccountAvailable? r.ReadString(RecipientAccount): string.Empty,

                    };
                }).ToList();
                return accountDefintionStatements;
            }
        }
    }
}