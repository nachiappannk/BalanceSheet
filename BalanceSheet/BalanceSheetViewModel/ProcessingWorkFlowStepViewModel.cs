using System.Collections.Generic;
using System.Linq;

namespace Nachiappan.BalanceSheetViewModel
{
    public class ProcessingWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private readonly DataStore _dataStore;

        public List<Information> InformationList { get; set; }

        private bool _canGoToNext = false;

        public string OverAllMessage { get; set; }

        public ProcessingWorkFlowStepViewModel(DataStore dataStore)
        {
            _dataStore = dataStore;

            var input = dataStore.GetPackage<InputForBalanceSheetComputation>();

            var logger = new Logger();

            JournalGateway gateway = new JournalGateway(input.CurrentJournalFileName);
            var statements = gateway.GetJournalStatements(logger, input.CurrentJournalSheetName);

            BalanceSheetGateway balanceSheetGateway = new BalanceSheetGateway(input.PreviousBalanceSheetFileName);
            var balanceSheetStatements = balanceSheetGateway.GetBalanceSheet(logger, input.PreviousBalanceSheetSheetName);

            InformationList = logger.InformationList;

            if (logger.InformationList.Any(x => x.GetType() == typeof(Error)))
            {
                OverAllMessage = "Please fix, there are some un ignorable error(s)";
                _canGoToNext = false;
            }
            else if(logger.InformationList.Any(x => x.GetType() == typeof(Warning)))
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