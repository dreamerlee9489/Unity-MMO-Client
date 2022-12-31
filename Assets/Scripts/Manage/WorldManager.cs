using Control;
using Control.FSM;
using Proto;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Manage
{
    public class WorldManager : MonoBehaviour
    {
        private readonly List<FsmController> _enemies = new();
        private readonly Dictionary<ulong, AppearRole> _players = new();

        public string csvFile = "";
        public List<FsmController> Enemies => _enemies;
        public Dictionary<ulong, AppearRole> Players => _players;

        private void Awake()
        {
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CEnemySyncPos, EnemyHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CAtkAnimEvent, AtkAnimEventHandler);
            EventManager.Instance.AddListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CEnemySyncPos, EnemyHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CAtkAnimEvent, AtkAnimEventHandler);
            EventManager.Instance.RemoveListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void RoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is RoleAppear proto)
            {
                foreach (Role role in proto.Role)
                {
                    ulong sn = role.Sn;
                    AppearRole appearRole = new();
                    appearRole.LoadRole(role);
                    _players.Add(sn, appearRole);
                }
            }
        }

        private void PlayerSyncStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is PlayerSyncState proto)
            {
                ulong playSn = proto.PlayerSn;
                if (_players.ContainsKey(playSn))
                {
                    AppearRole player = _players[playSn];
                    if (player.Obj != null)
                    {
                        PlayerController entity = player.Obj.GetComponent<PlayerController>();
                        entity.ParseSyncState(proto);
                    }
                }
            }
        }

        private void EnemyHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is EnemySyncPos proto)
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
                _enemies[proto.EnemyId].LinkPlayer(proto.IsLinker);
        }

        private void AtkAnimEventHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is AtkAnimEvent proto)
            {
                PlayerController player = _players[proto.PlayerSn].Obj.GetComponent<PlayerController>();
                FsmController enemy = _enemies[proto.EnemyId];
                if(proto.AtkEnemy)
                    enemy.SetHp(player, proto.CurrHp);
                else
                    player.SetHp(enemy, proto.CurrHp);
            }
        }

        private void PlayerLoadedCallback()
        {
            csvFile = $"{Application.streamingAssetsPath}/CSV/{csvFile}";
            using StreamReader reader = File.OpenText(csvFile);
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
                    enemyObj.NameBar.Name.text = "Enemy_" + enemyObj.id;
                    enemyObj.patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
                    enemyObj.patrolPath.transform.position = enemyObj.transform.position;
                    enemyObj.gameObject.SetActive(true);
                    _enemies.Add(enemyObj);
                });
            }
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