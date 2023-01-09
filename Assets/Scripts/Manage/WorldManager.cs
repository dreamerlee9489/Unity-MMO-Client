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
        private List<FsmController> _npcs = new();

        public string fileName = "";
        public Dictionary<ulong, GameItem> itemDict = new();
        public Dictionary<ulong, FsmController> npcDict = new();
        public Dictionary<ulong, Proto.AppearRole> roleDict = new();

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
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CReqJoinTeam, ReqJoinTeamHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CJoinTeamRes, JoinTeamResHandler);
        }

        private void Start()
        {
            _propPanel = UIManager.Instance.GetPanel<PropPanel>();
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
                    npcObj.SetNameBar(strs[6]);
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
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CReqJoinTeam, ReqJoinTeamHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CJoinTeamRes, JoinTeamResHandler);
        }

        private void AllRoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.AllRoleAppear proto)
            {
                foreach (Proto.Role role in proto.Roles)
                {
                    ulong sn = role.Sn;
                    if (roleDict.ContainsKey(sn))
                        roleDict[sn].Parse(role);
                    else
                    {
                        Proto.AppearRole appearRole = new();
                        appearRole.Parse(role);
                        appearRole.LoadRole(role);
                        roleDict.Add(sn, appearRole);
                    }
                }
            }
        }

        private void RoleDisappearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RoleDisappear proto)
            {
                ulong playSn = proto.Sn;
                if (roleDict.ContainsKey(playSn))
                {
                    foreach (var enemy in _npcs)
                        if (enemy.currState.Target == roleDict[playSn].obj)
                            enemy.ResetState();
                    Destroy(roleDict[playSn].obj);
                    roleDict.Remove(playSn);
                }
            }
        }

        private void SyncEntityStatusHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncEntityStatus proto)
            {
                if (npcDict.ContainsKey(proto.Sn))
                    npcDict[proto.Sn].ParseStatus(proto);
                else if (roleDict.ContainsKey(proto.Sn))
                    roleDict[proto.Sn].obj.ParseStatus(proto);
            }
        }

        private void SyncPlayerCmdHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncPlayerCmd proto)
            {
                if (roleDict.ContainsKey(proto.PlayerSn))
                {
                    Proto.AppearRole player = roleDict[proto.PlayerSn];
                    if (player.obj != null)
                        player.obj.ParseCmd(proto);
                }
            }
        }

        private void ReqSyncNpcHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqSyncNpc proto)
            {
                _npcs[proto.NpcId].Sn = proto.NpcSn;
                if (_npcs[proto.NpcId] != null)
                    npcDict.TryAdd(proto.NpcSn, _npcs[proto.NpcId]);
            }
        }

        private void SyncNpcPosHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncNpcPos proto)
                if(npcDict.ContainsKey(proto.NpcSn) && npcDict[proto.NpcSn] != null)
                    npcDict[proto.NpcSn].ParsePos(proto.Pos);
        }

        private void SyncFsmStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncFsmState proto)
            {
                PlayerController target = null;
                if (roleDict.ContainsKey(proto.PlayerSn))
                    target = roleDict[proto.PlayerSn].obj;
                if (npcDict.ContainsKey(proto.NpcSn) && npcDict[proto.NpcSn] != null)
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
            if (msg is Proto.PlayerKnap proto)
                GameManager.Instance.MainPlayer.Obj.ParsePlayerKnap(proto);
        }

        private void ReqJoinTeamHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqJoinTeam proto)
                if(GameManager.Instance.MainPlayer.Sn == proto.Responder)
                    roleDict[proto.Responder].obj.ParseJoinTeam(proto);
        }

        private void JoinTeamResHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.JoinTeamRes proto)
                if (GameManager.Instance.MainPlayer.Sn == proto.Applicant)
                    roleDict[proto.Applicant].obj.ParseJoinTeamRes(proto);
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