namespace Reflect.GameServer.Library.Messages
{
    public enum MessageAction : byte
    {
        Error = 10,
        None = 0,

        FacebookRegisterOrLogin = 1,
        MailRegister = 2,
        MailLogin = 3,

        UserInfo = 7,
        ConnectionOK = 8,

        GameJoin = 4,
        GameLeave = 5,
        GameData = 6
    }
}