using Control;
using Manage;
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
        public GameObject Obj => _obj;

        public void LoadPlayer(Net.Player proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Base.Gender;
            string path = _gender == Gender.Male ? "Entity/Player/Player_Knight" : "Entity/Player/Player_Warrior";
            _obj = Object.Instantiate(ResourceManager.Instance.Load<GameObject>(path));
            _obj.GetComponent<GameEntity>().NameBar.Name.text = _name;
            _obj.GetComponent<PlayerController>().Sn = _sn;
            _obj.SetActive(false);
            Object.DontDestroyOnLoad(_obj);
        }
    }
}
