using Control;
using Manage;
using UnityEngine;

namespace Proto
{
    public class AppearRole
    {
        protected ulong _sn;
        protected string _name;
        protected int _lv, _xp, _hp, _mp, _atk, _def;
        protected Vector3 _position;
        protected Gender _gender;
        protected PlayerController _obj;

        public string Name => _name;
        public PlayerController Obj => _obj;

        public void Parse(Role proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Gender;
            _lv = proto.Level;
            _xp = proto.Xp;
            _hp = proto.Hp;
            _mp = proto.Mp;
            _atk = proto.Atk;
            _def = proto.Def;
            _position.x = proto.Position.X;
            _position.y = proto.Position.Y;
            _position.z = proto.Position.Z;
        }

        public void LoadRole(Role proto)
        {
            if (_sn != GameManager.Instance.MainPlayer.Sn)
            {
                string path = _gender == Gender.Male ? "Entity/Player/Player_Knight" : "Entity/Player/Player_Warrior";
                ResourceManager.Instance.LoadAsync<GameObject>(path, (obj) =>
                {
                    _obj = Object.Instantiate(obj).GetComponent<PlayerController>();
                    _obj.Sn = _sn;
                    _obj.lv = _lv;
                    _obj.xp = _xp;
                    _obj.hp = _hp;
                    _obj.mp = _mp;
                    _obj.atk = _atk;
                    _obj.def = _def;
                    _obj.name = "Sync_" + _name;
                    _obj.SetNameBar(_name);
                    _obj.gameObject.SetActive(false);
                    _obj.transform.SetPositionAndRotation(_position, Quaternion.identity);
                    _obj.gameObject.SetActive(true);
                });
            }
            else
            {
                _obj = GameManager.Instance.MainPlayer.Obj;
                _obj.Sn = _sn;
                _obj.lv = _lv;
                _obj.xp = _xp;
                _obj.hp = _hp;
                _obj.mp = _mp;
                _obj.atk = _atk;
                _obj.def = _def;
                _obj.name = "MainPlayer";
                _obj.Agent.enabled = true;
                _obj.gameObject.SetActive(true);
                UIManager.Instance.FindPanel<PropPanel>().InitPanel();
                GameManager.Instance.VirtualCam.transform.position = _position + new Vector3(0, 6, -8);
                GameManager.Instance.VirtualCam.transform.rotation = Quaternion.AngleAxis(-50, Vector3.left);
                GameManager.Instance.VirtualCam.Follow = _obj.transform;
            }
        }        
    }
}
