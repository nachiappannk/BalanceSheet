using System;
using System.Collections.Generic;
using System.Linq;

namespace Nachiappan.BalanceSheetViewModel
{
    public class JournalGateway
    {
        private readonly string _inputFile;

        private List<string> _headings = new List<string>()
        {
             "S.No.", "Date",  "Account", "Tag",  "Description", "Credit" , "Debit" 
        };

        const int SerialNumber = 0;
        const int Date = 1;
        const int LedgerName = 2;
        const int Tag = 3;
        const int Description = 4;
        const int Credit = 5;
        const int Debit = 6;

        public JournalGateway(string inputFile)
        {
            _inputFile = inputFile;
        }

        public void WriteJournal(IList<JournalStatement> journalStatements)
        {
            using (ExcelSheetWriter writer = new ExcelSheetWriter(_inputFile, "Journal"))
            {
                int index = 0;

                writer.Write(index++, _headings.ToArray());
                writer.SetColumnsWidth(6, 12, 35, 45, 12, 12);
                writer.ApplyHeadingFormat(6);
                writer.WriteList(index, journalStatements.OrderBy(x => x.Date).ToList(),
                    (j, rowIndex) => new object[]
                    {
                        rowIndex - 1,
                        j.Date,
                        j.Description,
                        j.Tag,
                        j.DetailedDescription,
                        j.GetCreditValue(),
                        j.GetDebitValue(),
                    });

            }
        }


        public List<JournalStatement> GetJournalStatements(ILogger logger, string sheetName)
        {

            using (ExcelReader reader = new ExcelReader(_inputFile, sheetName, logger))
            {

                SheetHeadingVerifier.VerifyHeadingNames(logger, reader, _headings);

                var journalStatements = reader.ReadAllLines(1, (r) =>
                {
                    var isCreditAvailable = r.IsValueAvailable(Credit);
                    var credit = isCreditAvailable ? r.ReadDouble(Credit) : 0;
                    var isDebitAvailable = r.IsValueAvailable(Debit);
                    var debit = isDebitAvailable ? r.ReadDouble(Debit) : 0;
                    if (isCreditAvailable && isDebitAvailable)
                    {
                        if ((Math.Abs(credit) > 0.01) && (Math.Abs(debit) > 0.01))
                        {
                            logger.Log(MessageType.Warning, $"In file {r.FileName}, " +
                                                                   $"in sheet {r.SheetName}, " +
                                                                   $"in line no. {r.LineNumber}, " +
                                                                   "both credit and debit is having non zero values. Taking the difference as value");
                        }

                    }
                    if (!isCreditAvailable && !isDebitAvailable)
                    {
                        logger.Log(MessageType.Warning, $"In file{r.FileName}, " +
                                                               $"in sheet{r.SheetName}, " +
                                                               $"in line no. {r.LineNumber}, " +
                                                               "both credit and debit is not mentioned. Taking the value as 0");
                    }
                    return new JournalStatement()
                    {
                        Date = r.ReadDate(Date),
                        Description = r.ReadString(LedgerName),
                        Tag = r.ReadString(Tag),
                        DetailedDescription = r.ReadString(Description),
                        Value = credit - debit,
                    };
                }).ToList();
                return journalStatements;
            }
        }
    }



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

        public void WriteBalanceSheet(List<Statement> balanceSheetStatements)
        {
            var index = 0;
            using (var writer = new ExcelSheetWriter(_excelFileName, "BS"))
            {
                writer.Write(index++, headings.ToArray<object>());
                writer.SetColumnsWidth(6, 45, 12, 12, 12);
                writer.ApplyHeadingFormat(headings.Count);
                writer.WriteList(index, balanceSheetStatements, (b, rowIndex) => new object[]
                {
                    rowIndex - 1,
                    b.Description,
                    b.GetCreditValue(),
                    b.GetDebitValue(),
                });
                index = index + 1 + balanceSheetStatements.Count;
                writer.Write(index, new object[] { "", "Total", balanceSheetStatements.GetCreditTotal(), balanceSheetStatements.GetDebitTotal(),
                    balanceSheetStatements.GetTotal()});

            }
        }

        public List<Statement> GetBalanceSheet(ILogger logger, string sheetName)
        {
            using (ExcelReader reader = new ExcelReader(_excelFileName, sheetName, logger))
            {
                SheetHeadingVerifier.VerifyHeadingNames(logger, reader, headings);
                var balanceSheetStatements = reader.ReadAllLines(1, (r) =>
                {
                    var isValid = r.IsValueAvailable(SerialNumber);
                    if (!isValid) return new StatementWithValidity() { IsValid = false };

                    var isCreditAvailable = r.IsValueAvailable(Credit);
                    var credit = isCreditAvailable ? r.ReadDouble(Credit) : 0;
                    var isDebitAvailable = r.IsValueAvailable(Debit);
                    var debit = isDebitAvailable ? r.ReadDouble(Debit) : 0;
                    if (isCreditAvailable && isDebitAvailable)
                    {
                        logger.Log(MessageType.Warning, $"In file{r.FileName}, " +
                                                               $"in sheet{r.SheetName}, " +
                                                               $"in line no. {r.LineNumber}, " +
                                                               "both credit and debit is mentioned. Taking the difference as value");
                    }
                    if (!isCreditAvailable && !isDebitAvailable)
                    {
                        logger.Log(MessageType.Warning, $"In file{r.FileName}, " +
                                                               $"in sheet{r.SheetName}, " +
                                                               $"in line no. {r.LineNumber}, " +
                                                               "both credit and debit is not mentioned. Taking the value as 0");
                    }
                    return new StatementWithValidity()
                    {
                        IsValid = true,
                        Description = r.ReadString(Ledger),
                        Value = credit - debit,
                    };
                }).ToList();
                var balanceSheet = new List<Statement>();
                balanceSheet.AddRange(balanceSheetStatements.Where(x => x.IsValid).Select(y => new Statement() { Description = y.Description, Value = y.Value }));
                return balanceSheet;
            }
        }

        public class StatementWithValidity : Statement
        {
            public bool IsValid { get; set; }
        }
    }


    public class Statement : IHasValue
    {
        public string Description { get; set; }
        public double Value { get; set; }
    }


    


}