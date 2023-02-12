namespace Proto
{
    public class RoleInfo
    {
        private ulong _sn;
        private string _name;
        private int _level;
        private Gender _gender;

        public ulong Sn => _sn;
        public string Name => _name;
        public int Level => _level;
        public Gender Gender => _gender;

        public void ParseProto(PlayerLittle proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Gender;
            _level = proto.Level;
        }
    }
}
