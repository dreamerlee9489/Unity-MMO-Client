namespace Net
{
    public class RoleInfo
    {
        private ulong _id;
        private string _name;
        private int _level;
        private Net.Gender _gender;

        public ulong Id => _id;
        public string Name => _name;
        public int Level => _level;
        public Net.Gender Gender => _gender;

        public void ParseProto(Net.PlayerLittle proto)
        {
            _id = proto.Sn;
            _name = proto.Name;
            _gender = proto.Gender;
            _level = proto.Level;
        }
    }
}
