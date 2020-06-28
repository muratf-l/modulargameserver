using System;
using System.Collections.Generic;

namespace Reflect.GameServer.Library.Logging
{
    public abstract class ErrorLog
    {
        public void WriteError(string error)
        {
            WriteError(error, null, null, null);
        }

        public void WriteError(string error, Exception exception)
        {
            WriteError(error, exception.Message, exception.StackTrace, null);
        }

        public abstract void WriteError(string error, string details, string stacktrace,
            Dictionary<string, string> extraData);
    }
}