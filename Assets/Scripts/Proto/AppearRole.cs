﻿using Control;
using Manage;
using UnityEngine;

namespace Proto
{
    public class AppearRole
    {
        public ulong sn;
        public string name;
        public int lv, xp, hp, mp, atk, def;
        public Vector3 position;
        public Gender gender;
        public PlayerController obj;

        public void Parse(Role proto)
        {
            sn = proto.Sn;
            name = proto.Name;
            gender = proto.Gender;
            lv = proto.Level;
            xp = proto.Xp;
            hp = proto.Hp;
            mp = proto.Mp;
            atk = proto.Atk;
            def = proto.Def;
            position.x = proto.Position.X;
            position.y = proto.Position.Y;
            position.z = proto.Position.Z;
        }

        public void LoadRole()
        {
            if (sn != GameManager.Instance.mainPlayer.Sn)
            {
                string path = gender == Gender.Male ? "Entity/Player/Player_Knight" : "Entity/Player/Player_Warrior";
                ResourceManager.Instance.LoadAsync<GameObject>(path, (res) =>
                {
                    obj = Object.Instantiate(res).GetComponent<PlayerController>();
                    obj.Sn = sn;
                    obj.lv = lv;
                    obj.xp = xp;
                    obj.hp = hp;
                    obj.mp = mp;
                    obj.atk = atk;
                    obj.def = def;
                    obj.SetNameBar(name);
                    obj.name = "Sync_" + name;
                    obj.gameObject.SetActive(false);
                    obj.transform.position = position;
                    obj.gameObject.SetActive(true);
                    ReqSyncPlayer proto = new() { PlayerSn = sn };
                    NetManager.Instance.SendPacket(MsgId.C2SReqSyncPlayer, proto);
                });
            }
            else
            {
                obj = GameManager.Instance.mainPlayer.Obj;
                obj.Sn = sn;
                obj.lv = lv;
                obj.xp = xp;
                obj.hp = hp;
                obj.mp = mp;
                obj.atk = atk;
                obj.def = def;
                obj.SetNameBar(name);
                obj.name = "MainPlayer";
                obj.gameObject.SetActive(false);
                obj.transform.position = position;
                obj.gameObject.SetActive(true);
                UIManager.Instance.GetPanel<PropPanel>().InitPanel();
                GameManager.Instance.VirtualCam.transform.position = position + new Vector3(0, 6, -8);
                GameManager.Instance.VirtualCam.transform.rotation = Quaternion.AngleAxis(-50, Vector3.left);
                GameManager.Instance.VirtualCam.Follow = obj.transform;
            }
        }
    }
}
