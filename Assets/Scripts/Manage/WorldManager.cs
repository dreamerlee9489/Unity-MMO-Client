using Control;
using Control.FSM;
using Net;
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
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnemy, EnemyHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleDisAppear, RoleDisAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            EventManager.Instance.AddListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnemy, EnemyHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleDisAppear, RoleDisAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            EventManager.Instance.RemoveListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void RoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RoleAppear proto)
            {
                foreach (Proto.Role role in proto.Role)
                {
                    ulong sn = role.Sn;
                    if (_players.ContainsKey(sn))
                        _players[sn].Parse(role);
                    else
                    {
                        AppearRole appearRole = new();
                        appearRole.Parse(role);
                        appearRole.LoadObj();
                        _players.Add(sn, appearRole);
                    }
                }
            }
        }

        private void PlayerSyncStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerSyncState proto)
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
            if (msg is Proto.Enemy proto && _enemies.Count > 0)
            {
                int id = proto.Id;
                _enemies[id].ParseEnemy(proto);
            }
        }

        private void FsmSyncStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.FsmSyncState proto && _enemies.Count > 0)
            {
                FsmStateType type = (FsmStateType)proto.State;
                int code = proto.Code;
                int enemyId = proto.EnemyId;
                ulong playerSn = proto.PlayerSn;
                PlayerController player = null;
                if (_players.ContainsKey(playerSn))
                    player = _players[playerSn].Obj.GetComponent<PlayerController>();
                _enemies[enemyId].ParseSyncState(type, code, player);
            }
        }

        private void RoleDisAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RoleDisAppear proto)
            {
                ulong playSn = proto.Sn;
                if (_players.ContainsKey(playSn))
                {
                    Destroy(_players[playSn].Obj);
                    _players.Remove(playSn);
                }
            }
        }

        private void RequestLinkPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RequestLinkPlayer proto)
                _enemies[proto.EnemyId].LinkPlayer(proto.IsLinker);
        }

        private void PlayerLoadedCallback()
        {
            csvFile = Application.streamingAssetsPath + "/CSV/" + csvFile;
            using StreamReader reader = File.OpenText(csvFile);
            reader.ReadLine();
            string line;
            int id = 0;
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                ResourceManager.Instance.LoadAsync<GameObject>("Entity/Enemy/" + strs[1], (obj) =>
                {
                    FsmController enemyObj = Instantiate(obj).GetComponent<FsmController>();
                    enemyObj.gameObject.SetActive(false);
                    enemyObj.Id = id++;
                    enemyObj.Hp = int.Parse(strs[2]);
                    _enemies.Add(enemyObj);
                    Proto.RequestSyncEnemy proto = new()
                    {
                        PlayerSn = GameManager.Instance.MainPlayer.Sn,
                        EnemyId = enemyObj.Id
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SRequestSyncEnemy, proto);
                });
            }
        }
    }
}
