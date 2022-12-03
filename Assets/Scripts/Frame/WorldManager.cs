using Control;
using Control.FSM;
using Net;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Frame
{
    public class WorldManager : MonoBehaviour
    {
        private List<EnemyController> _enemies = new();
        private Dictionary<ulong, AppearRole> _players = new();

        public string csvFile = "";
        public List<EnemyController> Enemies => _enemies;
        public Dictionary<ulong, AppearRole> Players => _players;

        private void Awake()
        {
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CFsmChangeState, FsmChangeStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleDisAppear, RoleDisAppearHandler);
            PoolManager.Instance.Add(PoolType.PatrolPath, ResourceManager.Instance.Load<GameObject>("Entity/Enemy/PatrolPath"));
        }

        private void Start()
        {
            csvFile = Application.streamingAssetsPath + "/CSV/" + csvFile;
            using (StreamReader reader = File.OpenText(csvFile))
            {
                reader.ReadLine();
                string line;
                int id = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] strs = line.Split(',');
                    ResourceManager.Instance.LoadAsync<GameObject>("Entity/Enemy/" + strs[1], (obj) =>
                    {
                        EnemyController enemyObj = Instantiate(obj).GetComponent<EnemyController>();
                        enemyObj.gameObject.SetActive(false);
                        enemyObj.Id = id++;
                        enemyObj.Hp = int.Parse(strs[2]);
                        enemyObj.transform.position = GetPos(strs[3]);
                        enemyObj.gameObject.SetActive(true);
                        _enemies.Add(enemyObj);
                    });
                }
            }
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CFsmChangeState, FsmChangeStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleDisAppear, RoleDisAppearHandler);
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
                    }
                }
            }
        }

        private void PlayerSyncStateHandler(Google.Protobuf.IMessage msg)
        {
            Proto.PlayerSyncState proto = msg as Proto.PlayerSyncState;
            if (proto != null)
            {
                ulong playSn = proto.PlayerSn;
                if (_players.ContainsKey(playSn))
                {
                    AppearRole player = _players[playSn];
                    PlayerController entity = player.Obj.GetComponent<PlayerController>();
                    entity.ParseSyncState(proto);
                }
            }
        }

        private void EnemyListHandler(Google.Protobuf.IMessage msg)
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

        private void FsmChangeStateHandler(Google.Protobuf.IMessage msg)
        {
            Proto.FsmChangeState proto = msg as Proto.FsmChangeState;
            if (proto != null && _enemies.Count > 0)
            {
                AIStateType type = (AIStateType)proto.State;
                int code = proto.Code;
                int enemyId = proto.EnemyId;
                ulong playerSn = proto.PlayerSn;
                PlayerController player = null;
                if (_players.ContainsKey(playerSn))
                    player = _players[playerSn].Obj.GetComponent<PlayerController>();
                _enemies[enemyId].ChangeState(type, code, player);
            }
        }

        private void RoleDisAppearHandler(Google.Protobuf.IMessage msg)
        {
            Proto.RoleDisAppear proto = msg as Proto.RoleDisAppear;
            if (proto != null)
            {
                ulong playSn = proto.Sn;
                if (_players.ContainsKey(playSn))
                {
                    Destroy(_players[playSn].Obj);
                    _players.Remove(playSn);
                }
            }
        }
    }
}
