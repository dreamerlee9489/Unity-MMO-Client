namespace Net
{
    public enum EAppType
    {
        Client,
        Login,
        Game
    }

    public enum ENetState
    {
        NoConnect,
        Connecting,
        Connected,
        Disconnected,
    }

    public struct HttpJson
    {
        public string ip;
        public int port;
        public int returncode;
    }

    public class PacketHead
    {
        public const ushort SIZE = 6;
        public ushort msgId = 0;
    }
}
