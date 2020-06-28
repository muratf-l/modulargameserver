using System;
using System.Linq;

namespace Reflect.GameServer.CodeManager
{
    public class ExternalCodeManagerException : Exception
    {
        public ExternalCodeManagerException(string message, Exception innerException) : base(message, innerException)
        {
            char[] separator = {'.'};

            var moduleName = "   at " + GetType().Namespace?.Split(separator)[0] + ".";

            string[] textArray1 = {Environment.NewLine};

            var values = from l in innerException.ToString().Split(textArray1, StringSplitOptions.None)
                where !l.StartsWith(moduleName)
                select l;

            UserCodeFullStackTrace = string.Join(Environment.NewLine, values);
        }

        public string UserCodeFullStackTrace { get; }
    }
}