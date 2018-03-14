using System.Linq;

namespace Nachiappan.BalanceSheetViewModel
{
    public class ProcessingWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private readonly DataStore _dataStore;

        public ProcessingWorkFlowStepViewModel(DataStore dataStore)
        {
            _dataStore = dataStore;
        }
    }
}