using Control;
using Manage;
using UnityEngine;

namespace Proto
{
    public class PlayerInfo
    {
        protected ulong _sn;
        protected string _name;
        protected Gender _gender;
        protected PlayerController _obj = null;

        public ulong Sn => _sn;
        public string Name => _name;
        public PlayerController Obj => _obj;

        public void LoadPlayer(Player proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Base.Gender;
            string path = _gender == Gender.Male ? "Entity/Player/Player_Knight" : "Entity/Player/Player_Warrior";
            _obj = Object.Instantiate(ResourceManager.Instance.Load<PlayerController>(path));
            _obj.SetNameBar(_name);
            _obj.agent.enabled = false;
            _obj.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(_obj);
        }
    }
}
