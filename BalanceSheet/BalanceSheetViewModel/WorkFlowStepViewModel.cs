using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public abstract class WorkFlowStepViewModel
    {
        public string Name { get; set; }
        public DelegateCommand GoToPreviousCommand { get; set; }
        public DelegateCommand GoToNextCommand { get; set; }

    }
}