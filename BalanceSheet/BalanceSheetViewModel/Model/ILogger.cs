using System.Linq;
using System.Text;

namespace Nachiappan.BalanceSheetViewModel.Model
{
    public interface ILogger
    {
        void Log(MessageType type, string message);
    }

    public static class LoggerExtentions
    {
        public static void Log(this ILogger logger, MessageType type, params string[] messages)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var message in messages)
            {
                if (messages.Last() != message)
                {
                    stringBuilder.AppendLine(message);
                }
                else
                {
                    stringBuilder.Append(message);
                }

            }
            logger.Log(type, stringBuilder.ToString());
        }
    }

    public enum MessageType
    {
        
        Warning = 0,
        Error = 1,
    }
}