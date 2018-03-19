using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{

    public static class LedgerClassifer
    {

        private const string SimpleAccountPattern = "^([a-zA-Z0-9]+)$";
        private const string NominalAccountPattern = "^([a-zA-Z0-9]+)/([a-zA-Z0-9]+)$";
        private const string DoubleNominalAccountPattern = "^([a-zA-Z0-9]+/[a-zA-Z0-9]+)/([a-zA-Z0-9]+)$";

        private static Regex _simpleAccountPatternRegex = new Regex(SimpleAccountPattern, RegexOptions.IgnoreCase);
        private static Regex _nominalAccountPatternRegex = new Regex(NominalAccountPattern, RegexOptions.IgnoreCase);
        private static Regex _doubleNominalAccountPatternRegex = new Regex(DoubleNominalAccountPattern, RegexOptions.IgnoreCase);

        public static bool IsRealLedger(string name)
        {
            return _simpleAccountPatternRegex.IsMatch(name);
        }

        public static bool IsNominalLedger(string name)
        {
            return _nominalAccountPatternRegex.IsMatch(name);
        }

        public static bool IsDoubleNominalLedger(string name)
        {
            return _doubleNominalAccountPatternRegex.IsMatch(name);
        }

        public static string GetNominalPartOfName(string name)
        {
            if (IsNominalLedger(name))
            {
                var match = _nominalAccountPatternRegex.Match(name);
                return match.Groups[2].Value;
            }
            else if(IsDoubleNominalLedger(name))
            {
                var match = _doubleNominalAccountPatternRegex.Match(name);
                return match.Groups[2].Value;
            }
            else
            {
                throw new Exception();
            }
        }

        public static string GetBasePartOfName(string name)
        {
            if (IsNominalLedger(name))
            {
                var match = _nominalAccountPatternRegex.Match(name);
                return match.Groups[1].Value;
            }
            else if (IsDoubleNominalLedger(name))
            {
                var match = _doubleNominalAccountPatternRegex.Match(name);
                return match.Groups[1].Value;
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public class ProcessingWorkFlowStepViewModel : WorkFlowStepViewModel
    {

        public DelegateCommand ReadAgainCommand { get; set; }

        private readonly DataStore _dataStore;

        public List<Information> InformationList
        {
            get { return _informationList; }
            set
            {
                _informationList = value;
                FirePropertyChanged();
            }
        }

        private List<Information> _informationList;
        private string _overAllMessage;
        

        public string OverAllMessage
        {
            get { return _overAllMessage; }
            set
            {
                _overAllMessage = value;
                FirePropertyChanged();
            }
        }

        public ProcessingWorkFlowStepViewModel(DataStore dataStore, Action goToInputStep, 
            Action goToStatementVerifyingWorkFlowStep)
        {
            _dataStore = dataStore;
            Name = "Read Input And Process";
            GoToPreviousCommand = new DelegateCommand(goToInputStep);
            GoToNextCommand = new DelegateCommand(goToStatementVerifyingWorkFlowStep);
            ReadAgainCommand = new DelegateCommand(ProcessInputAndGenerateOutput);
            ProcessInputAndGenerateOutput();
        }

        private void ProcessInputAndGenerateOutput()
        {
            var input = _dataStore.GetPackage<InputForBalanceSheetComputation>
                (WorkFlowViewModel.InputParametersPackage);
            var startDate = input.AccountingPeriodStartDate;
            var endDate = input.AccountingPeriodEndDate;

            ReadInputStatements(input, out var statements, out var previousBalanceSheetStatements, out var errorsAndWarnings);
            PerformDateValidations(startDate, endDate, errorsAndWarnings);
            TrimJournalStatementsBasedOnDate(statements, startDate, endDate, out var dateTrimmedStatements);

            TrimJournalStatementsBasedOnAccountAndDescription(statements, out var accountTrimmedStatements);

            var groupedStatements = statements.GroupBy(x => x.Description + "#" + x.Tag).ToList();
            var trialBalanseStatements = groupedStatements.Select(x =>
            {
                return new TrialBalanceStatement()
                {
                    Account = x.ElementAt(0).Description,
                    Tag = x.ElementAt(0).Tag,
                    Value = x.Sum(y => y.Value),
                };
            }).ToList();


            LedgerHolder holder = new LedgerHolder(input.AccountingPeriodStartDate, input.AccountingPeriodEndDate);

            foreach (var balanceSheetStatement in previousBalanceSheetStatements)
            {
                holder.CreateRealLedger(balanceSheetStatement.Description, balanceSheetStatement.Value);
            }

            foreach (var journalStatement in statements)
            {
                var ledger = holder.GetLedger(journalStatement.Description);
                ledger.PostTransaction(journalStatement.Date, journalStatement.DetailedDescription, journalStatement.Value);
            }
            holder.CloseNominalLedgers();
            var allLedgers = holder.GetAllLedgers();
            var ledgerDictionary = allLedgers.ToDictionary(x => x.GetPrintableName(), x => x);


            var ledgers = holder.GetRealLedgers();

            var balanceSheetStatements = ledgers.Select(x => new Statement()
            {
                Description = x.GetPrintableName(),
                Value = x.GetLedgerValue(),
            }).ToList();


            PerformAccountTrimmedStatementsValidation(accountTrimmedStatements, errorsAndWarnings);
            PerformDateTrimmedStatementValidations(dateTrimmedStatements, errorsAndWarnings);
            PerformBalanceCheckValidationOnJournal(statements, errorsAndWarnings);
            PerformBalanceCheckValidationOnBalanceSheet(previousBalanceSheetStatements, errorsAndWarnings);

            InformationList = errorsAndWarnings;

            OverAllMessage = GetOverAllErrorMessage(errorsAndWarnings);
            

            _dataStore.PutPackage(trialBalanseStatements, WorkFlowViewModel.TrialBalancePackage);

            _dataStore.PutPackage(statements, WorkFlowViewModel.InputJournalPackage);

            var trimmedJournalStatements = dateTrimmedStatements.ToList();
            trimmedJournalStatements.AddRange(accountTrimmedStatements);
            _dataStore.PutPackage(trimmedJournalStatements, WorkFlowViewModel.TrimmedJournalPackage);
            _dataStore.PutPackage(previousBalanceSheetStatements, WorkFlowViewModel.PreviousBalanceSheetPacakge);
            _dataStore.PutPackage(balanceSheetStatements, WorkFlowViewModel.BalanceSheetPackage);
            _dataStore.PutPackage(ledgerDictionary , WorkFlowViewModel.LedgersPackage);
        }

        private static void PerformAccountTrimmedStatementsValidation(List<JournalStatement> filteredStatements, List<Information> errorsAndWarnings)
        {
            if (filteredStatements.Any())
            {
                errorsAndWarnings.Add(
                    new Warning()
                    {
                        Message = "Filtered journal statements as account or description is invalid"
                    });
            }
        }

        private static void TrimJournalStatementsBasedOnAccountAndDescription(List<JournalStatement> statements,
            out List<JournalStatement> filteredStatements)
        {
            var filteredStatements1 = statements.Where(x =>
            {
                var isNominalAccount = LedgerClassifer.IsNominalLedger(x.Description);
                var isRealAccount = LedgerClassifer.IsRealLedger(x.Description);
                var isDoubleNominalAccount = LedgerClassifer.IsDoubleNominalLedger(x.Description);

                return !isRealAccount && !isNominalAccount && !isDoubleNominalAccount;
            }).ToList();

            statements.RemoveAll(x => filteredStatements1.Contains(x));

            var filteredStatements2 = statements.Where(x => string.IsNullOrWhiteSpace(x.DetailedDescription)).ToList();
            statements.RemoveAll(x => filteredStatements2.Contains(x));

            filteredStatements = new List<JournalStatement>();
            filteredStatements.AddRange(filteredStatements1);
            filteredStatements.AddRange(filteredStatements2);
        }

        private static string GetOverAllErrorMessage(List<Information> errorsAndWarnings)
        {
            if (errorsAndWarnings.Any(x => x.GetType() == typeof(Error)))
            {
                return "Please fix, there are some un ignorable error(s)";
            }
            if (errorsAndWarnings.Any(x => x.GetType() == typeof(Warning)))
            {
                return "Please review, there are some error(s)";
            }
            return "Congrats!!! There are no errors";
        }

        private static void PerformDateTrimmedStatementValidations
            (List<JournalStatement> trimmedStatements, List<Information> errorsAndWarnings)
        {
            if (trimmedStatements.Any())
            {
                errorsAndWarnings.Add(new Warning()
                {
                    Message = "Journal statement(s) have been trimmed as they are outside the accouting period",
                });
            }
        }

        private static void TrimJournalStatementsBasedOnDate(List<JournalStatement> statements,
            DateTime startDate, DateTime endDate,
            out List<JournalStatement> trimmedStatements)
        {
            var statementsBeforePeriod = statements.Where(x => x.Date < startDate).ToList();
            statements.RemoveAll(x => statementsBeforePeriod.Contains(x));
            var statementsAfterPeriod = statements.Where(x => x.Date > endDate).ToList();
            statements.RemoveAll(x => statementsAfterPeriod.Contains(x));
            trimmedStatements = statementsAfterPeriod.ToList();
            trimmedStatements.AddRange(statementsBeforePeriod);
        }

        private static void ReadInputStatements(InputForBalanceSheetComputation input,
            out List<JournalStatement> journalStatements,
            out List<Statement> balanceSheetStatements, 
            out List<Information> readErrorAndWarnings)
        {
            var logger = new Logger();

            JournalGateway gateway = new JournalGateway(input.CurrentJournalFileName);
            journalStatements = gateway.GetJournalStatements(logger, input.CurrentJournalSheetName);
            
            TrimTimeComponentFromDate(journalStatements);
            TrimDescription(journalStatements);
            TrimDetailedDescription(journalStatements);
            TrimTag(journalStatements);
            CorrectAccountNesting(journalStatements);


            BalanceSheetGateway balanceSheetGateway = new BalanceSheetGateway(input.PreviousBalanceSheetFileName);
            balanceSheetStatements = balanceSheetGateway.GetBalanceSheet(logger, input.PreviousBalanceSheetSheetName);
            TrimBalanceSheetDescription(balanceSheetStatements);

            readErrorAndWarnings = logger.InformationList.ToList();
        }

        



        private static void CorrectAccountNesting(List<JournalStatement> journalStatements)
        {
            //journalStatements.ForEach(x => x.Description = x.Description.Replace(@"/","#"));
            journalStatements.ForEach(x => x.Description = x.Description.Replace(@"\", "/"));
        }

        private static void TrimBalanceSheetDescription(List<Statement> balanceSheetStatements)
        {
            balanceSheetStatements.ForEach(x =>
            {
                if (string.IsNullOrWhiteSpace(x.Description))
                {
                    x.Description = string.Empty;
                }

                x.Description = x.Description.Trim();
            });
        }

        private static void TrimDetailedDescription(List<JournalStatement> journalStatements)
        {
            journalStatements.ForEach(x =>
            {
                if (string.IsNullOrWhiteSpace(x.DetailedDescription))
                {
                    x.DetailedDescription = string.Empty;
                }
            });
            journalStatements.ForEach(x => x.DetailedDescription = x.DetailedDescription.Trim());
        }

        private static void TrimTag(List<JournalStatement> journalStatements)
        {
            journalStatements.ForEach(x =>
            {
                if (string.IsNullOrWhiteSpace(x.Tag))
                {
                    x.Tag = string.Empty;
                }

                ;
            });
            journalStatements.ForEach(x => x.Tag = x.Tag.Trim());
        }

        private static void TrimDescription(List<JournalStatement> journalStatements)
        {
            journalStatements.ForEach(x =>
            {
                if (string.IsNullOrWhiteSpace(x.Description))
                {
                    x.Description = string.Empty;
                }

                ;
            });
            journalStatements.ForEach(x => x.Description = x.Description.Trim());
        }

        private void PerformBalanceCheckValidationOnBalanceSheet(List<Statement> balanceSheetStatements, List<Information> informationList)
        {
            var sumOfBalanceSheetStatement = balanceSheetStatements.Sum(x => x.Value);
            if (!sumOfBalanceSheetStatement.IsZero())
            {
                informationList.Add(new Error()
                {
                    Message = "The previous balance sheet is not balanced. The error in the input balance sheet is " +
                              sumOfBalanceSheetStatement
                });
            }
        }

        private void PerformBalanceCheckValidationOnJournal(List<JournalStatement> statements, List<Information> informationList)
        {
            var sumOfJournalStatement = statements.Sum(x => x.Value);
            if (!sumOfJournalStatement.IsZero())
            {
                informationList.Add(new Error()
                {
                    Message = "The input journal is not balanced. The sum of the journal entry is " + sumOfJournalStatement
                });
            }
        }

        private static void TrimTimeComponentFromDate(List<JournalStatement> statements)
        {
            statements.ForEach(x => x.Date = x.Date.Date);
        }

        private void PerformDateValidations(DateTime startDate, DateTime endDate, List<Information> informationList)
        {
            if (startDate > endDate)
            {
                informationList.Add(new Error()
                {
                    Message = "The accounting period start date is later than end date",
                });
            }
            else if ((endDate - startDate).TotalDays < 29)
            {
                informationList.Add(new Warning()
                {
                    Message = "The accounting period is less than 29 days",
                });
            }
        }
    }


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
                    nominaLedgers.Add(name, new NominalLedger(name, GetNominalPartOfName(name), GetBasePartOfName(name)));
                }
                return nominaLedgers[name];
            }

            if (IsDoubleNominalLedgerName(name))
            {
                if (!doubleNominalLedgers.ContainsKey(name))
                {
                    doubleNominalLedgers.Add(name, new NominalLedger(name, GetNominalPartOfName(name), GetBasePartOfName(name)));
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

    

    public class LedgerStatement : IHasValue
    {
        public int SerialNumber { get; set; }
        public double Value { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }

    public class RealLedger : ILedger
    {
        private readonly string _accountName;
        private double ledgerValue = 0;

        private List<LedgerStatement> ledgerStatements = new List<LedgerStatement>();

        public RealLedger(string accountName, DateTime openingDate, double value)
        {
            _accountName = accountName;
            ledgerStatements.Add(new LedgerStatement()
            {
                Date = openingDate,
                Description = "Opening",
                SerialNumber = 1,
                Value =  value
            });
            ledgerValue = value;

        }

        public string GetPrintableName()
        {
            return _accountName;
        }

       
        public void PostTransaction(DateTime date, string statement, double value)
        {
            var count = ledgerStatements.Count + 1;
            ledgerStatements.Add(new LedgerStatement() { Date = date, Description = statement, SerialNumber = count, Value = value });
            ledgerValue += value;
        }

        public double GetLedgerValue()
        {
            return ledgerValue;
        }

        public List<LedgerStatement> GetLedgerStatements()
        {
            return ledgerStatements.ToList();
        }

        public string GetLedgerType()
        {
            if (ledgerValue > 0) return "Equity or Liability";
            else return "Asset";
        }
    }


    public interface ILedger
    {
        string GetPrintableName();
        void PostTransaction(DateTime date, string statement, double value);
        double GetLedgerValue();
        List<LedgerStatement> GetLedgerStatements();

        string GetLedgerType();
    }

    public class NominalLedger : ILedger
    {
        private readonly string _accountName;
        private readonly string _nominalName;
        private readonly string _baseName;
        private readonly DateTime _openingDate;
        private double ledgerValue = 0;

        private List<LedgerStatement> ledgerStatements = new List<LedgerStatement>();

        public List<LedgerStatement> GetLedgerStatements()
        {
            return ledgerStatements.ToList();
        }

        public string GetLedgerType()
        {
            return "Nominal Account";
        }

        public NominalLedger(string accountName, string nominalName, string baseName)
        {
            _accountName = accountName;
            _nominalName = nominalName;
            _baseName = baseName;
        }

        public string GetPrintableName()
        {
            return _accountName.Replace("/","-");
        }

        public void PostTransaction(DateTime date, string statement, double value)
        {
            var count = ledgerStatements.Count + 1;
            ledgerStatements.Add(new LedgerStatement() { Date = date, Description = statement, SerialNumber = count, Value = value });
            ledgerValue += value;
        }

        public double GetLedgerValue()
        {
            return ledgerValue;
        }
    }
    

    public class Logger : ILogger
    {

        public List<Information> InformationList = new List<Information>();
        public void Log(MessageType type, string message)
        {
            if(type == MessageType.Warning)
                InformationList.Add(new Warning(){Message = message});

            if (type == MessageType.Error)
                InformationList.Add(new Error() { Message = message });
        }
    }


    public class Information
    {
        public string Message { get; set; }
    }


    public class Warning : Information
    {
    }

    public class Error : Information
    {
    }

    public class TrialBalanceStatement : IHasValue
    {
        public string Account { get; set; }
        public string Tag { get; set; }
        public double Value { get; set; }
    }
}