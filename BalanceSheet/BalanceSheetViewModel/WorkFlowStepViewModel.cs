using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nachiappan.BalanceSheetViewModel.Annotations;
using Prism.Commands;

namespace Nachiappan.BalanceSheetViewModel
{
    public abstract class WorkFlowStepViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public DelegateCommand GoToPreviousCommand { get; set; }
        public DelegateCommand GoToNextCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}