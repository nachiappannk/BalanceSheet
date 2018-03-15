namespace Nachiappan.BalanceSheetViewModel
{
    public interface ILogger
    {
        void Log(MessageType type, string message);
    }

    public enum MessageType
    {
        Information = 0,
        Warning = 1,
        IgnorableError = 2,
        Error = 3,
    }
}