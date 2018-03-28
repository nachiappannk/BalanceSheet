using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model.Excel;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.ExcelGateway
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
                writer.SetColumnsWidth(6, 12, 35, 30, 45, 12, 12);
                writer.ApplyHeadingFormat(_headings.Count);
                writer.WriteList(index, journalStatements.OrderBy(x => x.Date).ToList(),
                    (j, rowIndex) => new object[]
                    {
                        rowIndex - 1,
                        j.Date,
                        j.Account,
                        j.Tag,
                        j.Description,
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
                        if (!credit.IsZero() && !debit.IsZero())
                        {
                            logger.Log(MessageType.Warning, $"In file {r.FileName}, " ,
                                                                   $"in sheet {r.SheetName}, " ,
                                                                   $"in line no. {r.LineNumber}, " ,
                                                                   "both credit and debit is having non zero values. Taking the difference as value");
                        }

                    }
                    if (!isCreditAvailable && !isDebitAvailable)
                    {
                        logger.Log(MessageType.Warning, $"In file {r.FileName}, " ,
                                                               $"in sheet {r.SheetName}, " ,
                                                               $"in line no. {r.LineNumber}, " ,
                                                               "both credit and debit is not mentioned. Taking the value as 0");
                    }
                    return new JournalStatement()
                    {
                        Date = r.ReadDate(Date),
                        Account = r.ReadString(LedgerName),
                        Tag = r.ReadString(Tag),
                        Description = r.ReadString(Description),
                        Value = credit - debit,
                    };
                }).ToList();
                return journalStatements;
            }
        }
    }
}