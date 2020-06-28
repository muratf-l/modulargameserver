namespace Reflect.GameServer.Data.Models
{
    public enum UserRegisterMethod : byte
    {
        Unknow = 0,
        Mail = 10,
        Facebook = 20
    }

    public enum UserRegisterStatus : byte
    {
        Guest = 0,
        Registered = 10
    }

    public enum UserOnlineStatus : byte
    {
        Offline = 0,
        Online = 10
    }
}