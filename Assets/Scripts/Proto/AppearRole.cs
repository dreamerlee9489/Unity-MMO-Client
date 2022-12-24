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
        protected GameObject _obj;

        public ulong Sn => _sn;
        public string Name => _name;
        public GameObject Obj => _obj;

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
                    _obj = Object.Instantiate(obj);
                    _obj.SetActive(false);
                    _obj.name = "Sync_" + _name;
                    _obj.transform.SetPositionAndRotation(_position, Quaternion.identity);
                    _obj.GetComponent<GameEntity>().NameBar.Name.text = _name;
                    _obj.GetComponent<PlayerController>().Sn = _sn;
                    _obj.SetActive(true);
                });
            }
            else
            {
                _obj = GameManager.Instance.MainPlayer.Obj;
                _obj.name = "MainPlayer";
                _obj.transform.SetPositionAndRotation(_position, Quaternion.identity);
                _obj.SetActive(true);
                GameManager.Instance.VirtualCam.transform.position = _position + new Vector3(0, 6, -8);
                GameManager.Instance.VirtualCam.transform.rotation = Quaternion.AngleAxis(-50, Vector3.left);
                GameManager.Instance.VirtualCam.Follow = _obj.transform;
                EventManager.Instance.Invoke(EEventType.PlayerLoaded);
            }
        }
    }
}
