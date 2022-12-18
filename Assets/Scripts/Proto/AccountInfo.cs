using System.Collections.Generic;

namespace Proto
{
    public class AccountInfo
    {
        private string _account;
        private readonly List<RoleInfo> _players = new();

        public string Account => _account;
        public List<RoleInfo> Players => _players;

        public void ParseProto(PlayerList proto)
        {
            _account = proto.Account;
            _players.Clear();

            foreach (PlayerLittle roleProto in proto.Player)
            {
                RoleInfo role = new();
                role.ParseProto(roleProto);
                _players.Add(role);
            }
        }
    }
}
