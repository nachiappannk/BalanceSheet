using Prism.Interactivity.InteractionRequest;

namespace Nachiappan.BalanceSheetViewModel
{
    public class FileSaveAsNotification : INotification
    {
        public string Title { get; set; }
        public object Content { get; set; }
        public string DefaultFileName { get; set; }
        public string OutputFileName { get; set; }
        public string OutputFileExtention { get; set; }
        public string OutputFileExtentionFilter { get; set; }
        public bool FileSaved { get; set; }
    }
}