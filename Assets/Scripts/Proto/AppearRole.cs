using Control;
using Manage;
using UnityEngine;

namespace Proto
{
    public class AppearRole
    {
        protected ulong _sn;
        protected string _name;
        protected Vector3 _position;
        protected Gender _gender;
        protected PlayerController _obj;

        public ulong Sn => _sn;
        public string Name => _name;
        public PlayerController Obj => _obj;

        public void LoadRole(Role proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Gender;
            _position.x = proto.Position.X;
            _position.y = proto.Position.Y;
            _position.z = proto.Position.Z;

            if (_sn != GameManager.Instance.MainPlayer.Sn)
            {
                string path = _gender == Gender.Male ? "Entity/Player/Player_Knight" : "Entity/Player/Player_Warrior";
                ResourceManager.Instance.LoadAsync<GameObject>(path, (obj) =>
                {
                    _obj = Object.Instantiate(obj).GetComponent<PlayerController>();
                    _obj.sn = _sn;
                    _obj.lv = proto.Level;
                    _obj.xp = proto.Xp;
                    _obj.hp = proto.Hp;
                    _obj.mp = proto.Mp;
                    _obj.atk = proto.Atk;
                    _obj.def = proto.Def;
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
                _obj.sn = _sn;
                _obj.lv = proto.Level;
                _obj.xp = proto.Xp;
                _obj.hp = proto.Hp;
                _obj.mp = proto.Mp;
                _obj.atk = proto.Atk;
                _obj.def = proto.Def;
                _obj.name = "MainPlayer";
                _obj.transform.SetPositionAndRotation(_position, Quaternion.identity);
                _obj.gameObject.SetActive(true);
                UIManager.Instance.FindPanel<PropPanel>().InitPanel();
                GameManager.Instance.VirtualCam.transform.position = _position + new Vector3(0, 6, -8);
                GameManager.Instance.VirtualCam.transform.rotation = Quaternion.AngleAxis(-50, Vector3.left);
                GameManager.Instance.VirtualCam.Follow = _obj.transform;
            }
        }
    }
}
