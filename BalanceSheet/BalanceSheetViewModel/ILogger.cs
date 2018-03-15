﻿namespace Nachiappan.BalanceSheetViewModel
{
    public interface ILogger
    {
        void Log(MessageType type, string message);
    }

    public enum MessageType
    {
        
        Warning = 0,
        Error = 1,
    }
}