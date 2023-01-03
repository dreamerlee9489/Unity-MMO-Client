using Control;
using Control.FSM;
using Proto;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Manage
{
    public class WorldManager : MonoBehaviour
    {
        private readonly List<FsmController> _enemies = new();
        private readonly Dictionary<ulong, AppearRole> _players = new();
        private PropPanel _propPanel;

        public string fileName = "";
        public List<FsmController> Enemies => _enemies;
        public Dictionary<ulong, AppearRole> Players => _players;

        private void Awake()
        {
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CAllRoleAppear, AllRoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CEnemyPushPos, EnemyPushPosHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CPlayerSyncCmd, PlayerSyncCmdHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CAtkAnimEvent, AtkAnimEventHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CDropItemList, DropItemListHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CGetPlayerKnap, PlayerKnapHandler);

            fileName = $"{Application.streamingAssetsPath}/CSV/{fileName}.csv";
            using StreamReader reader = File.OpenText(fileName);
            reader.ReadLine();
            string line;
            int id = 0;
            Vector3 pos = Vector3.zero;
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                ResourceManager.Instance.LoadAsync<GameObject>("Entity/Enemy/" + strs[1], (obj) =>
                {
                    FsmController enemyObj = Instantiate(obj).GetComponent<FsmController>();
                    enemyObj.gameObject.SetActive(false);
                    enemyObj.id = id++;
                    enemyObj.lv = int.Parse(strs[2]);
                    enemyObj.hp = int.Parse(strs[3]);
                    enemyObj.atk = int.Parse(strs[4]);
                    enemyObj.transform.position = pos.Parse(strs[5]);
                    enemyObj.SetNameBar("Enemy_" + enemyObj.id);
                    enemyObj.patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
                    enemyObj.patrolPath.transform.position = enemyObj.transform.position;
                    enemyObj.gameObject.SetActive(true);
                    _enemies.Add(enemyObj);
                });
            }
        }

        private void Start()
        {
            _propPanel = UIManager.Instance.FindPanel<PropPanel>();
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CAllRoleAppear, AllRoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CEnemyPushPos, EnemyPushPosHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CPlayerSyncCmd, PlayerSyncCmdHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CAtkAnimEvent, AtkAnimEventHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CDropItemList, DropItemListHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CGetPlayerKnap, PlayerKnapHandler);
        }

        private void AllRoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is AllRoleAppear proto)
            {
                foreach (Role role in proto.Roles)
                {
                    ulong sn = role.Sn;
                    AppearRole appearRole = new();
                    appearRole.LoadRole(role);
                    _players.Add(sn, appearRole);
                }
            }
        }

        private void PlayerSyncCmdHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is PlayerSyncCmd proto)
            {
                ulong playSn = proto.PlayerSn;
                if (_players.ContainsKey(playSn))
                {
                    AppearRole player = _players[playSn];
                    if (player.Obj != null)
                    {
                        PlayerController entity = player.Obj.GetComponent<PlayerController>();
                        entity.ParseSyncCmd(proto);
                    }
                }
            }
        }

        private void EnemyPushPosHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is EnemyPushPos proto)
            {
                int id = proto.Id;
                if (_enemies.Count >= id)
                    _enemies[id].ParseSyncPos(proto);
            }
        }

        private void FsmSyncStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is FsmSyncState proto && _enemies.Count > 0)
            {
                FsmStateType type = (FsmStateType)proto.State;
                int code = proto.Code;
                int enemyId = proto.EnemyId;
                ulong playerSn = proto.PlayerSn;
                PlayerController player = null;
                if (_players.ContainsKey(playerSn))
                    player = _players[playerSn].Obj.GetComponent<PlayerController>();
                if (_enemies.Count >= enemyId)
                    _enemies[enemyId].ParseSyncState(type, code, player);
            }
        }

        private void RoleDisappearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is RoleDisappear proto)
            {
                ulong playSn = proto.Sn;
                if (_players.ContainsKey(playSn))
                {
                    foreach (var enemy in _enemies)
                        if (enemy.currState.Target == _players[playSn].Obj.GetComponent<PlayerController>())
                            enemy.ResetState();
                    Destroy(_players[playSn].Obj);
                    _players.Remove(playSn);
                }
            }
        }

        private void RequestLinkPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is RequestLinkPlayer proto && _enemies.Count > 0)
                _enemies[proto.EnemyId].LinkPlayer(proto.Linker);
        }

        private void AtkAnimEventHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is AtkAnimEvent proto)
            {
                PlayerController player = _players[proto.PlayerSn].Obj.GetComponent<PlayerController>();
                if (proto.EnemyId == -1)
                {
                    player.SetHp(player, proto.CurrHp);
                    _propPanel.UpdateHp(proto.CurrHp);
                    return;
                }
                FsmController enemy = _enemies[proto.EnemyId];
                if (proto.AtkEnemy)
                {
                    enemy.SetHp(player, proto.CurrHp);
                }
                else
                {
                    player.SetHp(enemy, proto.CurrHp);
                    _propPanel.UpdateHp(proto.CurrHp);
                }
            }
        }

        private void DropItemListHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is DropItemList itemList)
                _enemies[itemList.EnemyId].DropItems(itemList);
        }

        private void PlayerKnapHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is PlayerKnap playerKnap)
                GameManager.Instance.MainPlayer.Obj.ParsePlayerKnap(playerKnap);
        }
    }
}

public static partial class Utils
{
    public static Vector3 Parse(this Vector3 pos, string str)
    {
        str = str[1..^1];
        string[] strs = str.Split(';');
        pos.x = float.Parse(strs[0]);
        pos.y = float.Parse(strs[1]);
        pos.z = float.Parse(strs[2]);
        return pos;
    }
}