using UnityEngine;

namespace Net
{
    public class PlayerInfo
    {
        protected ulong _sn;
        protected string _name;
        protected Net.Gender _gender;
        protected GameObject _obj = null;
        public ulong Sn => _sn;
        public string Name => _name;

        public void SetGameObject(GameObject obj) => _obj = obj;

        public GameObject GetGameObject() => _obj;

        public void Parse(Net.Player proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Base.Gender;
        }
    }
}
