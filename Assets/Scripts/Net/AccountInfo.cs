using System.Collections.Generic;

namespace Net
{
    public class AccountInfo
    {
        private string _account;
        private readonly List<RoleInfo> _players = new();

        public string Account => _account;
        public List<RoleInfo> Players => _players;

        public void ParseProto(Net.PlayerList proto)
        {
            _account = proto.Account;
            _players.Clear();

            foreach (Net.PlayerLittle roleProto in proto.Player)
            {
                RoleInfo role = new();
                role.ParseProto(roleProto);
                _players.Add(role);
            }
        }
    }
}
