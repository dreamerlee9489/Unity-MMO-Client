using Control;
using Control.FSM;
using Items;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Manage
{
    public class WorldManager : MonoBehaviour
    {
        private PropPanel _propPanel;
        private readonly List<FsmController> _npcs = new();

        public string fileName = "";
        public Dictionary<ulong, GameItem> itemDict = new();
        public Dictionary<ulong, FsmController> npcDict = new();
        public Dictionary<ulong, Proto.AppearRole> playerDict = new();

        private void Awake()
        {
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CAllRoleAppear, AllRoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncEntityStatus, SyncEntityStatusHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CReqSyncNpc, ReqSyncNpcHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncNpcPos, SyncNpcPosHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncFsmState, SyncFsmStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncPlayerCmd, SyncPlayerCmdHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CReqLinkPlayer, ReqLinkPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CDropItemList, DropItemListHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CGetPlayerKnap, PlayerKnapHandler);
        }

        private void Start()
        {
            _propPanel = UIManager.Instance.FindPanel<PropPanel>();
            fileName = $"{Application.streamingAssetsPath}/CSV/{fileName}.csv";
            using StreamReader reader = File.OpenText(fileName);
            reader.ReadLine();
            string line;
            int id = 0;
            Vector3 pos = Vector3.zero;
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                ResourceManager.Instance.LoadAsync<GameObject>("Entity/NPC/" + strs[1], (obj) =>
                {
                    FsmController npcObj = Instantiate(obj).GetComponent<FsmController>();
                    npcObj.gameObject.SetActive(false);
                    npcObj.id = id++;
                    npcObj.lv = int.Parse(strs[2]);
                    npcObj.hp = int.Parse(strs[3]);
                    npcObj.atk = int.Parse(strs[4]);
                    npcObj.transform.position = pos.Parse(strs[5]);
                    npcObj.initHp = npcObj.hp;
                    npcObj.SetNameBar("Enemy_" + id);
                    npcObj.patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
                    npcObj.patrolPath.transform.position = npcObj.transform.position;
                    npcObj.gameObject.SetActive(true);
                    _npcs.Add(npcObj);
                    Proto.ReqSyncNpc proto = new()
                    {
                        NpcId = npcObj.id,
                        NpcSn = 0
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SReqSyncNpc, proto);
                });
            }
        }

        private void OnDestroy()
        {
            _npcs.Clear();
            npcDict.Clear();
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CAllRoleAppear, AllRoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncEntityStatus, SyncEntityStatusHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CReqSyncNpc, ReqSyncNpcHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncNpcPos, SyncNpcPosHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncFsmState, SyncFsmStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncPlayerCmd, SyncPlayerCmdHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CReqLinkPlayer, ReqLinkPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CDropItemList, DropItemListHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CGetPlayerKnap, PlayerKnapHandler);
        }

        private void AllRoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.AllRoleAppear proto)
            {
                foreach (Proto.Role role in proto.Roles)
                {
                    ulong sn = role.Sn;
                    if (playerDict.ContainsKey(sn))
                        playerDict[sn].Parse(role);
                    else
                    {
                        Proto.AppearRole appearRole = new();
                        appearRole.Parse(role);
                        appearRole.LoadRole(role);
                        playerDict.Add(sn, appearRole);
                    }
                }
            }
        }

        private void RoleDisappearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RoleDisappear proto)
            {
                ulong playSn = proto.Sn;
                if (playerDict.ContainsKey(playSn))
                {
                    foreach (var enemy in _npcs)
                        if (enemy.currState.Target == playerDict[playSn].Obj)
                            enemy.ResetState();
                    Destroy(playerDict[playSn].Obj);
                    playerDict.Remove(playSn);
                }
            }
        }

        private void SyncEntityStatusHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncEntityStatus proto)
            {
                if (npcDict.ContainsKey(proto.Sn))
                    npcDict[proto.Sn].ParseStatus(proto);
                else
                    playerDict[proto.Sn].Obj.ParseStatus(proto);
            }
        }

        private void SyncPlayerCmdHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncPlayerCmd proto)
            {
                if (playerDict.ContainsKey(proto.PlayerSn))
                {
                    Proto.AppearRole player = playerDict[proto.PlayerSn];
                    if (player.Obj != null)
                        player.Obj.ParseSyncCmd(proto);
                }
            }
        }

        private void ReqSyncNpcHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqSyncNpc proto)
            {
                _npcs[proto.NpcId].Sn = proto.NpcSn;
                npcDict.TryAdd(proto.NpcSn, _npcs[proto.NpcId]);
            }
        }

        private void SyncNpcPosHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncNpcPos proto)
                npcDict[proto.NpcSn].ParsePos(proto.Pos);
        }

        private void SyncFsmStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncFsmState proto)
            {
                PlayerController target = null;
                if (playerDict.ContainsKey(proto.PlayerSn))
                    target = playerDict[proto.PlayerSn].Obj;
                npcDict[proto.NpcSn].ParseFsmState((StateType)proto.State, proto.Code, target);
            }
        }

        private void ReqLinkPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqLinkPlayer proto)
            {
                if (!npcDict.ContainsKey(proto.NpcSn) && _npcs.Count > 0)
                    npcDict.TryAdd(proto.NpcSn, _npcs[proto.NpcId]);
                if (npcDict.ContainsKey(proto.NpcSn))
                    npcDict[proto.NpcSn].LinkPlayer(proto.Linker);
            }
        }

        private void DropItemListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.DropItemList proto)
                npcDict[proto.NpcSn].DropItems(proto);
        }

        private void PlayerKnapHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerKnap playerKnap)
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