using UnityEngine;

namespace Net
{
    public class Player
    {
        protected ulong _sn;
        protected string _name;
        protected Proto.Gender _gender;
        protected GameObject _obj = null;
        public ulong Sn => _sn;

        public void SetGameObject(GameObject obj)
        {
            _obj = obj;
        }

        public GameObject GetGameObject()
        {
            return _obj;
        }

        public void Parse(Proto.Player proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Base.Gender;
        }
    }
}
