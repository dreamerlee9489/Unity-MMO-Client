using Control;
using Control.FSM;
using Net;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Frame
{
    public class WorldManager : MonoBehaviour
    {
        private List<FsmController> _enemies = new();
        private Dictionary<ulong, AppearRole> _players = new();

        public string csvFile = "";
        public List<FsmController> Enemies => _enemies;
        public Dictionary<ulong, AppearRole> Players => _players;

        private void Awake()
        {
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleDisAppear, RoleDisAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            EventManager.Instance.AddListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CFsmSyncState, FsmSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CPlayerSyncState, PlayerSyncStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleDisAppear, RoleDisAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRequestLinkPlayer, RequestLinkPlayerHandler);
            EventManager.Instance.RemoveListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
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
            if (msg is Proto.RoleAppear proto)
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

        private void EnemyListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.EnemyList proto && _enemies.Count > 0)
            {
                var enemyList = proto.Enemies;
                for (int i = 0; i < enemyList.Count; ++i)
                {
                    _enemies[i].Id = i;
                    _enemies[i].ParseEnemy(enemyList[i]);
                }
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
                _enemies[proto.EnemyId].LinkPlayer();
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
                GameObject obj = ResourceManager.Instance.Load<GameObject>("Entity/Enemy/" + strs[1]);
                FsmController enemyObj = Instantiate(obj).GetComponent<FsmController>();
                enemyObj.gameObject.SetActive(false);
                enemyObj.Id = id++;
                enemyObj.Hp = int.Parse(strs[2]);
                enemyObj.transform.position = GetPos(strs[3]);
                enemyObj.gameObject.SetActive(true);
                _enemies.Add(enemyObj);
            }
            Proto.RequestSyncEnemies proto = new() { PlayerSn = GameManager.Instance.MainPlayer.Sn };
            NetManager.Instance.SendPacket(Proto.MsgId.C2SRequestSyncEnemies, proto);
        }
    }
}
