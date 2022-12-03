using Control;
using Frame;
using UnityEngine;

namespace Net
{
    public class AppearRole
    {
        protected ulong _sn;
        protected string _name;
        protected Vector3 _position;
        protected Proto.Gender _gender;
        protected GameObject _obj;

        public ulong Sn => _sn;
        public GameObject Obj => _obj;

        public void LoadObj()
        {
            string path = _gender == Proto.Gender.Male ? "Entity/Player/Player_Knight" : "Entity/Player/Player_Warrior";
            ResourceManager.Instance.LoadAsync<GameObject>(path, (obj) =>
            {
                _obj = Object.Instantiate(obj);
                _obj.SetActive(false);
                _obj.transform.SetPositionAndRotation(_position, Quaternion.identity);
                if (_sn != GameManager.Instance.MainPlayer.Sn)
                    _obj.name = "Sync_" + _name;
                else
                {
                    _obj.name = "MainPlayer";
                    GameManager.Instance.MainPlayer.SetGameObject(_obj);
                    GameManager.Instance.VirtualCamera.transform.SetPositionAndRotation(_position + new Vector3(0, 6, -8), Quaternion.AngleAxis(-45, Vector3.left));
                    GameManager.Instance.VirtualCamera.Follow = _obj.transform;
                    Object.DontDestroyOnLoad(_obj);
                    Proto.RequestSyncEnemies proto = new() { PlayerSn = _sn };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SRequestSyncEnemies, proto);
                }
                _obj.GetComponent<GameEntity>().NameBar.Name.text = _name;
                _obj.GetComponent<PlayerController>().Sn = _sn;
                _obj.SetActive(true);
            });
        }

        public void Parse(Proto.Role proto)
        {
            _sn = proto.Sn;
            _name = proto.Name;
            _gender = proto.Gender;
            _position.x = proto.Position.X;
            _position.y = proto.Position.Y;
            _position.z = proto.Position.Z;
        }
    }
}
