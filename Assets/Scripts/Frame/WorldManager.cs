using Control;
using Google.Protobuf;
using Net;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Frame
{
    public class WorldManager : MonoBehaviour
    {
        private List<EnemyController> _enemies = new();
        private Dictionary<ulong, AppearRole> _players = new();

        public string csvFile = "";
        public List<EnemyController> Enemies => _enemies;

        private void Awake()
        {
            csvFile = Application.streamingAssetsPath + "/CSV/" + csvFile;
            using (StreamReader reader = File.OpenText(csvFile))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] strs = line.Split(',');
                    ResourceManager.Instance.LoadAsync<GameObject>("Entity/Enemy/" + strs[1], (obj) =>
                    {
                        EnemyController enemyObj = Instantiate(obj).GetComponent<EnemyController>();
                        enemyObj.gameObject.SetActive(false);
                        enemyObj.Entity.Hp = int.Parse(strs[2]);
                        enemyObj.transform.position = GetPos(strs[3]);
                        enemyObj.gameObject.SetActive(true);
                        _enemies.Add(enemyObj);
                    });
                }
            }

            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CMove, SyncMoveHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CMove, SyncMoveHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
        }

        private Vector3 GetPos(string posStr)
        {
            Vector3 pos = Vector3.zero;
            posStr = posStr.Substring(1, posStr.Length - 2);
            string[] strs = posStr.Split(';');
            pos.x = float.Parse(strs[0]);
            pos.y = float.Parse(strs[1]);
            pos.z = float.Parse(strs[2]);
            return pos;
        }

        private void RoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            print("GameManager.RoleAppearHandler");
            Proto.RoleAppear proto = msg as Proto.RoleAppear;
            if (proto != null)
            {
                foreach (Proto.Role role in proto.Role)
                {
                    ulong sn = role.Sn;
                    if (_players.ContainsKey(sn))
                        _players[sn].Parse(role);
                    else
                    {
                        AppearRole appearRole = new AppearRole();
                        appearRole.Parse(role);
                        appearRole.LoadObj();
                        _players.Add(sn, appearRole);
                        Debug.Log("sync player sn=" + sn + " world =" + SceneManager.GetActiveScene().name);
                    }
                }
            }
        }

        private void SyncMoveHandler(Google.Protobuf.IMessage msg)
        {
            Proto.Move proto = msg as Proto.Move;
            if (proto != null)
            {
                ulong playSn = proto.PlayerSn;
                int enemyId = proto.EnemyId;
                if (enemyId == -1 && _players.ContainsKey(playSn))
                {
                    AppearRole player = _players[playSn];
                    Entity entity = player.Obj.GetComponent<Entity>();
                    entity.CornerPoints.AddRange(proto.Position);
                }
                else
                {
                    _enemies[enemyId].Entity.CornerPoints.AddRange(proto.Position);
                }
            }
        }

        private void EnemyListHandler(IMessage msg)
        {
            Proto.EnemyList proto = msg as Proto.EnemyList;
            if (proto != null)
            {
                var enemyList = proto.Enemies;
                for (int i = 0; i < enemyList.Count; ++i)
                {
                    _enemies[i].Id = i;
                    _enemies[i].ParseProto(enemyList[i]);
                }
            }
        }
    }
}
