namespace Reflect.GameServer.Library.SocketLibrary
{
    public abstract class BandwidthMeter
    {
        public abstract void Closed();
        public abstract void Received(int bytes, int messages);
        public abstract void Sent(int bytes, int messages);
    }
}