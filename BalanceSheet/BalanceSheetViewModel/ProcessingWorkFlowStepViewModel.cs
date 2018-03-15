using System.Linq;

namespace Nachiappan.BalanceSheetViewModel
{
    public class ProcessingWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private readonly DataStore _dataStore;

        public ProcessingWorkFlowStepViewModel(DataStore dataStore)
        {
            _dataStore = dataStore;

            var input = dataStore.GetPackage<InputForBalanceSheetComputation>();
            
            JournalGateway gateway = new JournalGateway(input.CurrentJournalFileName);
            var statements = gateway.GetJournalStatements(new Logger(), input.CurrentJournalSheetName);

            
        }
    }

    public class Logger : ILogger
    {
        public void Log(MessageType type, string message)
        {
        }
    }
}