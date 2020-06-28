using System;
using System.Diagnostics;

namespace Reflect.GameServer.Library.Logging
{
    public static class LogService
    {
        public static void WriteDebug(string msg = "")
        {
#if DEBUG

            Console.WriteLine(msg);
            Debug.WriteLine(msg);
#endif
        }

        public static void WriteDebug(object sender, string msg = "")
        {
#if DEBUG


            Console.WriteLine(msg);
            Debug.WriteLine(msg);
#endif
        }

        public static void Error(string message, params object[] args)
        {

        }
    }
}