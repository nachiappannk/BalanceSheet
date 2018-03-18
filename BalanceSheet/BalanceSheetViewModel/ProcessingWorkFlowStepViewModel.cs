using System;
using System.Collections.Generic;
using System.Linq;
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
            var input = _dataStore.GetPackage<InputForBalanceSheetComputation>
                (WorkFlowViewModel.InputParametersPackage);
            var startDate = input.AccountingPeriodStartDate;
            var endDate = input.AccountingPeriodEndDate;

            ReadInputStatements(input, out var statements, out var balanceSheetStatements, out var errorsAndWarnings);
            PerformDateValidations(startDate, endDate, errorsAndWarnings);
            TrimJournalStatements(statements, startDate, endDate, out var trimmedStatements);

            PerformTrimmedStatementValidations(trimmedStatements, errorsAndWarnings);
            PerformBalanceCheckValidationOnJournal(statements, errorsAndWarnings);
            PerformBalanceCheckValidationOnBalanceSheet(balanceSheetStatements, errorsAndWarnings);

            InformationList = errorsAndWarnings;

            OverAllMessage = GetOverAllErrorMessage(errorsAndWarnings);
            
            _dataStore.PutPackage(statements, WorkFlowViewModel.InputJournalPackage);
            _dataStore.PutPackage(trimmedStatements, WorkFlowViewModel.TrimmedJournalPackage);
            _dataStore.PutPackage(balanceSheetStatements, WorkFlowViewModel.PreviousBalanceSheetPacakge);
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

        private static void PerformTrimmedStatementValidations
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

        private static void TrimJournalStatements(List<JournalStatement> statements,
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

            BalanceSheetGateway balanceSheetGateway = new BalanceSheetGateway(input.PreviousBalanceSheetFileName);
            balanceSheetStatements = balanceSheetGateway.GetBalanceSheet(logger, input.PreviousBalanceSheetSheetName);

            TrimTimeComponentFromDate(journalStatements);

            readErrorAndWarnings = logger.InformationList.ToList();
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
}