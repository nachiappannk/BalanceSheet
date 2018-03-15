using System;
using System.Windows;
using System.Windows.Interactivity;
using Nachiappan.BalanceSheetViewModel;
using Prism.Interactivity.InteractionRequest;

namespace Nachiappan.BalanceSheet
{
    public class SaveFileAction : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            InteractionRequestedEventArgs args = parameter as InteractionRequestedEventArgs;
            if (args == null) return;
            var fileSaveAsNotification = args.Context as FileSaveAsNotification;
            if (fileSaveAsNotification == null) return;
            Microsoft.Win32.SaveFileDialog dlg =
                new Microsoft.Win32.SaveFileDialog
                {
                    Title = fileSaveAsNotification.Title,
                    FileName = fileSaveAsNotification.DefaultFileName,
                    DefaultExt = fileSaveAsNotification.OutputFileExtention,
                    Filter = fileSaveAsNotification.OutputFileExtentionFilter,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

            fileSaveAsNotification.OutputFileName = String.Empty;
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                fileSaveAsNotification.OutputFileName = dlg.FileName;
                fileSaveAsNotification.FileSaved = true;
            }
            else
            {
                fileSaveAsNotification.FileSaved = false;
            }
        }
    }
}