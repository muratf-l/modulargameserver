namespace Reflect.GameServer.GameManager
{
    public enum GameEvent : byte
    {
        GotMessage = 0,

        GameJoin = 2,
        GameLeft = 3,
        GameStarted = 5,
        GameData = 6,
        GameClosed = 7,

        UserRegister = 10,
        UserLogin = 11,

        SocketConnected = 12,
        SocketDisconnected = 13,
    }
}