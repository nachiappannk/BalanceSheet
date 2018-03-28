using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Excel;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.ExcelGateway
{
    public class BalanceSheetGateway
    {
        private readonly string _excelFileName;

        private readonly List<string> headings = new List<string>()
        {
            "S.No.", "Account", "Liability & Equity" , "Assets" , "Total"
        };
        private const int SerialNumber = 0;
        private const int Ledger = 1;
        private const int Credit = 2;
        private const int Debit = 3;
        private const int Total = 4;

        public BalanceSheetGateway(string excelFileName)
        {
            _excelFileName = excelFileName;

        }

        public void WriteBalanceSheet(List<BalanceSheetStatement> balanceSheetStatements, string sheetName)
        {
            var index = 0;
            using (var writer = new ExcelSheetWriter(_excelFileName, sheetName))
            {
                writer.Write(index++, headings.ToArray<object>());
                writer.SetColumnsWidth(6, 45, 12, 12, 12);
                writer.ApplyHeadingFormat(headings.Count);
                writer.WriteList(index, balanceSheetStatements, (b, rowIndex) => new object[]
                {
                    rowIndex - 1,
                    b.Account,
                    b.GetCreditValue(),
                    b.GetDebitValue(),
                });
                index = index + 1 + balanceSheetStatements.Count;
                writer.Write(index, new object[] { "", "Total", balanceSheetStatements.GetCreditTotal(), balanceSheetStatements.GetDebitTotal(),
                    balanceSheetStatements.GetTotal()});

            }
        }

        public List<BalanceSheetStatement> GetBalanceSheet(ILogger logger, string sheetName)
        {
            using (ExcelReader reader = new ExcelReader(_excelFileName, sheetName, logger))
            {
                SheetHeadingVerifier.VerifyHeadingNames(logger, reader, headings);
                var balanceSheetStatements = reader.ReadAllLines(1, (r) =>
                {
                    
                    var isValid = r.IsValueAvailable(SerialNumber);
                    if (!isValid) return Tuple.Create(new BalanceSheetStatement(), false);

                    var isCreditAvailable = r.IsValueAvailable(Credit);
                    var credit = isCreditAvailable ? r.ReadDouble(Credit) : 0;
                    var isDebitAvailable = r.IsValueAvailable(Debit);
                    var debit = isDebitAvailable ? r.ReadDouble(Debit) : 0;
                    if (!credit.IsZero() && !debit.IsZero())
                    {
                        logger.Log(MessageType.Warning, $"In file {r.FileName}, ",
                                                        $"in sheet {r.SheetName}, " ,
                                                        $"in line no. {r.LineNumber}, " ,
                                                        "both credit and debit is mentioned. Taking the difference as value");
                    }
                    if (!isCreditAvailable && !isDebitAvailable)
                    {
                        logger.Log(MessageType.Warning, $"In file {r.FileName}, " ,
                                                        $"in sheet {r.SheetName}, " ,
                                                        $"in line no. {r.LineNumber}, " ,
                                                        "both credit and debit is not mentioned. Taking the value as 0");
                    }

                    return Tuple.Create(new BalanceSheetStatement()
                    {
                        Account = r.ReadString(Ledger),
                        Value = credit - debit,
                    }, true);
                }).ToList();
                var balanceSheet = new List<BalanceSheetStatement>();
                balanceSheet.AddRange(balanceSheetStatements.Where(x => x.Item2).Select(y => y.Item1));
                return balanceSheet;
            }
        }
    }
}