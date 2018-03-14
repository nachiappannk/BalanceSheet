using System;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public class AboutApplicationWorkFlowStepViewModel : WorkFlowStepViewModel
    {
        private readonly Action _nextStep;

        public AboutApplicationWorkFlowStepViewModel(Action nextStep)
        {
            _nextStep = nextStep;

            GoToPreviousCommand = new DelegateCommand(() => { }, () => false);
            GoToNextCommand = new DelegateCommand(nextStep, ()=> true);
        }
    }
}