using System;

namespace Reflect.GameServer.Library.Logging
{
    public class PlayerIoError : Exception
    {
        public PlayerIoError(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; }
    }
}