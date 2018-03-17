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

        private bool _canGoToNext = false;
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
            var input = _dataStore.GetPackage<InputForBalanceSheetComputation>("inputparameters");

            var logger = new Logger();

            JournalGateway gateway = new JournalGateway(input.CurrentJournalFileName);
            var statements = gateway.GetJournalStatements(logger, input.CurrentJournalSheetName);
            _dataStore.PutPackage(statements,"inputjournal");

            BalanceSheetGateway balanceSheetGateway = new BalanceSheetGateway(input.PreviousBalanceSheetFileName);
            var balanceSheetStatements = balanceSheetGateway.GetBalanceSheet(logger, input.PreviousBalanceSheetSheetName);
            _dataStore.PutPackage(balanceSheetStatements,"inputpreviousbalancesheet");

            InformationList = logger.InformationList;

            if (logger.InformationList.Any(x => x.GetType() == typeof(Error)))
            {
                OverAllMessage = "Please fix, there are some un ignorable error(s)";
                _canGoToNext = false;
            }
            else if (logger.InformationList.Any(x => x.GetType() == typeof(Warning)))
            {
                OverAllMessage = "Please review, there are some error(s)";
                _canGoToNext = true;
            }
            else
            {
                OverAllMessage = "Congrats!!! There are no errors";
                _canGoToNext = true;
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