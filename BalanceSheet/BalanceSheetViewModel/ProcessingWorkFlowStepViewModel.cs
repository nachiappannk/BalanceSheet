using System;
using System.Collections.Generic;
using System.Linq;
using Nachiappan.BalanceSheetViewModel.Model;
using Nachiappan.BalanceSheetViewModel.Model.Account;
using Nachiappan.BalanceSheetViewModel.Model.ExcelGateway;
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

            var logger = new Logger();

            var journalStatements = JournalReader.ReadJournalStatements
                (input.CurrentJournalFileName, input.CurrentJournalSheetName, logger);

            var previousBalanceSheetStatements = BalanceSheetReader.ReadBalanceSheetStatements
                (input.PreviousBalanceSheetFileName, input.PreviousBalanceSheetSheetName, logger);
            
            var trimmedJournalStatements = JournalStatementsCleaner
                .RemoveInvalidJournalStatements(journalStatements, startDate, endDate, logger);

            var trimmedBalanceSheetStatements = BalanceSheetStatementsCleaner
                .RemovedInvalidBalanceSheetStatements(previousBalanceSheetStatements, logger);

            _dataStore.PutPackage(trimmedBalanceSheetStatements, WorkFlowViewModel.TrimmedPreviousBalanceSheetStatements);
            _dataStore.PutPackage(journalStatements, WorkFlowViewModel.InputJournalStatementsPackageDefintion);
            _dataStore.PutPackage(trimmedJournalStatements, WorkFlowViewModel.TrimmedJournalStatementsPackageDefintion);
            _dataStore.PutPackage(previousBalanceSheetStatements, WorkFlowViewModel.PreviousBalanceSheetStatementsPackageDefinition);
            
            ValidateAccountingPeriod(startDate, endDate, logger);
            ValidateJournalStatements(journalStatements, logger);
            ValidateBalanceSheetStatements(previousBalanceSheetStatements, logger);

            logger.InformationList.Sort((a, b) =>
            {
                if (a.GetType() == b.GetType()) return 0;
                if (a.GetType() == typeof(Error)) return -1;
                return 1;
            });

            InformationList = logger.InformationList.ToList();
            OverAllMessage = GetOverAllErrorMessage(logger.InformationList.ToList());







        }

        private static string GetOverAllErrorMessage(List<Information> errorsAndWarnings)
        {
            var errorCount = errorsAndWarnings.Count(x => x.GetType() == typeof(Error));
            var warningCount = errorsAndWarnings.Count(x => x.GetType() == typeof(Warning));

            if (errorCount > 1 && warningCount > 1) return "Please check inputs. There are few errors and warnings";
            if (errorCount > 1 && warningCount == 1) return "Please check inputs. There are few errors and one warning";
            if (errorCount > 1 && warningCount == 0) return "Please check inputs. There are few errors";
            if (errorCount == 1 && warningCount > 1) return "Please check inputs. There is an errors and a few warnings";
            if (errorCount == 1 && warningCount == 1) return "Please check inputs. There is an errors and a warning";
            if (errorCount == 1 && warningCount == 0) return "Please check inputs. There is an few errors";
            if (errorCount == 0 && warningCount > 1) return "Please review inputs. There are a few warnings";
            if (errorCount == 0 && warningCount == 1) return "Please review inputs. There a warning";
            return "Congrats!!! There are no errors or warnings. Please verify output";
        }


        private void ValidateBalanceSheetStatements(List<BalanceSheetStatement> balanceSheetStatements, ILogger logger)
        {
            var sumOfBalanceSheetStatement = balanceSheetStatements.Sum(x => x.Value);
            if (!sumOfBalanceSheetStatement.IsZero())
            {
                logger.Log(MessageType.Error,
                    "The previous balance sheet is not balanced. The error in the input balance sheet is " +
                           sumOfBalanceSheetStatement);
            }
        }

        private void ValidateJournalStatements(List<JournalStatement> statements, ILogger logger)
        {
            var sumOfJournalStatement = statements.Sum(x => x.Value);
            if (!sumOfJournalStatement.IsZero())
            {
                logger.Log(MessageType.Error,
                    "The input journal is not balanced. The sum of the journal entry is " + sumOfJournalStatement);
            }
        }

        private void ValidateAccountingPeriod(DateTime startDate, DateTime endDate,ILogger logger)
        {
            if (startDate > endDate)
            {
                logger.Log(MessageType.Error,"The accounting period start date is later than end date");
            }
            else if ((endDate - startDate).TotalDays < 29)
            {
                logger.Log(MessageType.Warning, "The accounting period is less than 29 days");
            }
        }
    }


    public class Logger : ILogger
    {

        public List<Information> InformationList = new List<Information>();
        public void Log(MessageType type, string message)
        {
            if(type == MessageType.Warning)
                InformationList.Add(Information.CreateWarning(message));

            if (type == MessageType.Error)
                InformationList.Add(Information.CreateError(message));
        }
    }


    public abstract class Information
    {
        public string Message { get; set; }

        public static Information CreateError(string message)
        {
            return new Error() {Message = message};
        }

        public static Information CreateWarning(string message)
        {
            return new Warning() { Message = message };
        }
    }


    public class Warning : Information
    {
    }

    public class Error : Information
    {
    }
}