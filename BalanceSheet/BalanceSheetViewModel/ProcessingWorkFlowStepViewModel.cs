using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            TrimJournalStatementsBasedOnDate(statements, startDate, endDate, out var dateTrimmedStatements);

            var simpleAccountPattern = "^[a-zA-Z0-9]+$";
            var nominalAccountPattern = "^[a-zA-Z0-9]+/[a-zA-Z0-9]+$";
            var doubleNominalAccountPattern = "^[a-zA-Z0-9]+/[a-zA-Z0-9]+/[a-zA-Z0-9]+$";

            Regex simpleAccountPatternRegex = new Regex(simpleAccountPattern, RegexOptions.IgnoreCase);
            Regex nominalAccountPatternRegex = new Regex(nominalAccountPattern, RegexOptions.IgnoreCase);
            Regex doubleNominalAccountPatternRegex = new Regex(doubleNominalAccountPattern, RegexOptions.IgnoreCase);

            TrimJournalStatementsBasedOnAccountAndDescription(statements, simpleAccountPatternRegex, 
                nominalAccountPatternRegex, doubleNominalAccountPatternRegex, out var accountTrimmedStatements);





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


            PerformAccountTrimmedStatementsValidation(accountTrimmedStatements, errorsAndWarnings);
            PerformDateTrimmedStatementValidations(dateTrimmedStatements, errorsAndWarnings);
            PerformBalanceCheckValidationOnJournal(statements, errorsAndWarnings);
            PerformBalanceCheckValidationOnBalanceSheet(balanceSheetStatements, errorsAndWarnings);


            

            InformationList = errorsAndWarnings;

            OverAllMessage = GetOverAllErrorMessage(errorsAndWarnings);
            

            _dataStore.PutPackage(trialBalanseStatements, WorkFlowViewModel.TrialBalancePackage);

            _dataStore.PutPackage(statements, WorkFlowViewModel.InputJournalPackage);

            var trimmedJournalStatements = dateTrimmedStatements.ToList();
            trimmedJournalStatements.AddRange(accountTrimmedStatements);
            _dataStore.PutPackage(trimmedJournalStatements, WorkFlowViewModel.TrimmedJournalPackage);

            _dataStore.PutPackage(balanceSheetStatements, WorkFlowViewModel.PreviousBalanceSheetPacakge);
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
            Regex simpleAccountPatternRegex,
            Regex nominalAccountPatternRegex,
            Regex doubleNominalAccountPatternRegex, 
            out List<JournalStatement> filteredStatements)
        {
            var filteredStatements1 = statements.Where(x =>
            {
                var isNominalAccount = nominalAccountPatternRegex.IsMatch(x.Description);
                var isRealAccount = simpleAccountPatternRegex.IsMatch(x.Description);
                var isDoubleNominalAccount = doubleNominalAccountPatternRegex.IsMatch(x.Description);

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