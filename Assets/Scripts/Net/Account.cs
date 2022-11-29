using System.Collections.Generic;

namespace Net
{
    public class PlayerInfo
    {
        private ulong _id;
        private string _name;
        private int _level;
        private Proto.Gender _gender;

        public ulong Id => _id;
        public string Name => _name;
        public int Level => _level;
        public Proto.Gender Gender => _gender;

        public void ParseProto(Proto.PlayerLittle proto)
        {
            _id = proto.Sn;
            _name = proto.Name;
            _gender = proto.Gender;
            _level = proto.Level;
        }
    }

    public class AccountInfo
    {
        private string _account;
        private readonly List<PlayerInfo> _players = new();

        public string Account => _account;
        public List<PlayerInfo> Players => _players;

        public void ParseProto(Proto.PlayerList proto)
        {
            _account = proto.Account;
            _players.Clear();

            foreach (Proto.PlayerLittle roleProto in proto.Player)
            {
                PlayerInfo role = new PlayerInfo();
                role.ParseProto(roleProto);
                _players.Add(role);
            }
        }
    }
}
