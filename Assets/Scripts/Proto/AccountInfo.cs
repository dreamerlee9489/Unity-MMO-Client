using System.Collections.Generic;

namespace Proto
{
    public class AccountInfo
    {
        private string _account;
        private readonly List<RoleInfo> _roles = new();

        public string Account => _account;
        public List<RoleInfo> Roles => _roles;

        public void ParseProto(PlayerList proto)
        {
            _account = proto.Account;
            _roles.Clear();

            foreach (PlayerLittle roleProto in proto.Player)
            {
                RoleInfo role = new();
                role.ParseProto(roleProto);
                _roles.Add(role);
            }
        }
    }
}
