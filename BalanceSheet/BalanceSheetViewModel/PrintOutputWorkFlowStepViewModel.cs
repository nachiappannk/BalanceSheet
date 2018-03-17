using System;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public class PrintOutputWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        public PrintOutputWorkFlowStepViewModel(DataStore dataStore, Action goToPrevious)
        {
            GoToPreviousCommand = new DelegateCommand(goToPrevious);
            GoToNextCommand = new DelegateCommand(CloseApplication);

        }

        private void CloseApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}