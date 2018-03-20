using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Ledger;
using Nachiappan.BalanceSheetViewModel.Model.Statements;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
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
            var input = _dataStore.GetPackage(WorkFlowViewModel.InputParametersPackageDefinition);
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
            
            var ledgers = holder.GetRealLedgers();

            var balanceSheetStatements = ledgers.Select(x => new BalanceSheetStatement()
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
            

            _dataStore.PutPackage(trialBalanseStatements, WorkFlowViewModel.TrialBalanceStatementsPackageDefinition);

            _dataStore.PutPackage(statements, WorkFlowViewModel.InputJournalStatementsPackageDefintion);

            var trimmedJournalStatements = dateTrimmedStatements.ToList();
            trimmedJournalStatements.AddRange(accountTrimmedStatements);
            _dataStore.PutPackage(trimmedJournalStatements, WorkFlowViewModel.TrimmedJournalStatementsPackageDefintion);
            _dataStore.PutPackage(previousBalanceSheetStatements, WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition);
            _dataStore.PutPackage(balanceSheetStatements, WorkFlowViewModel.BalanceSheetStatementsPackageDefinition);
            _dataStore.PutPackage(allLedgers, WorkFlowViewModel.LedgersPackageDefinition);
        }

        private static void PerformAccountTrimmedStatementsValidation(List<TrimmedJournalStatement> filteredStatements, List<Information> errorsAndWarnings)
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
            out List<TrimmedJournalStatement> filteredStatements)
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

            filteredStatements = new List<TrimmedJournalStatement>();
            filteredStatements.AddRange(filteredStatements1.Select(x => new TrimmedJournalStatement(x, "The account is invalid")));
            filteredStatements.AddRange(filteredStatements2.Select(x => new TrimmedJournalStatement(x, "The description is invalid")));
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
            (List<TrimmedJournalStatement> trimmedStatements, List<Information> errorsAndWarnings)
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
            out List<TrimmedJournalStatement> trimmedStatements)
        {
            var statementsBeforePeriod = statements.Where(x => x.Date < startDate).ToList();
            statements.RemoveAll(x => statementsBeforePeriod.Contains(x));
            var statementsAfterPeriod = statements.Where(x => x.Date > endDate).ToList();
            statements.RemoveAll(x => statementsAfterPeriod.Contains(x));
            trimmedStatements = statementsAfterPeriod
                .Select(x => new TrimmedJournalStatement(x, "After the end of  accounting period")).ToList();
            trimmedStatements.AddRange(statementsBeforePeriod.Select(x =>
                new TrimmedJournalStatement(x, "Before the start of accounting period")));
        }

        private static void ReadInputStatements(InputForBalanceSheetComputation input,
            out List<JournalStatement> journalStatements,
            out List<BalanceSheetStatement> balanceSheetStatements, 
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
            journalStatements.ForEach(x => x.Description = x.Description.Replace(@"\", "/"));
        }

        private static void TrimBalanceSheetDescription(List<BalanceSheetStatement> balanceSheetStatements)
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

        private void PerformBalanceCheckValidationOnBalanceSheet(List<BalanceSheetStatement> balanceSheetStatements, List<Information> informationList)
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