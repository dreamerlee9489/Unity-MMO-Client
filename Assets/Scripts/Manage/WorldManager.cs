using Control;
using Control.BT;
using Control.FSM;
using Items;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Manage
{
    public class WorldManager : MonoBehaviour
    {
        //private readonly List<FsmController> _npcs = new();
        private readonly List<BtController> _npcs = new();

        public string fileName = "";
        public Dictionary<ulong, GameItem> itemDict = new();
        //public Dictionary<ulong, FsmController> npcDict = new();
        public Dictionary<ulong, BtController> npcDict = new();
        public Dictionary<ulong, Proto.AppearRole> roleDict = new();

        private void Start()
        {
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
                    //var npcObj = Instantiate(obj).GetComponent<FsmController>();
                    var npcObj = Instantiate(obj).GetComponent<BtController>();
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
                    _npcs.Add(npcObj);
                });
            }
        }

        private void OnDestroy()
        {
            _npcs.Clear();
            npcDict.Clear();
            itemDict.Clear();
            roleDict.Clear();
        }

        public void ParseAllRoleAppear(Proto.AllRoleAppear proto)
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

        public void ParseRoleDisappear(Proto.RoleDisappear proto)
        {
            ulong playSn = proto.Sn;
            if (roleDict.ContainsKey(playSn) && roleDict[playSn] != null)
            {
                //foreach (var enemy in _npcs)
                //    if (enemy.currState.Target == roleDict[playSn].obj)
                //        enemy.ResetState();
                Destroy(roleDict[playSn].obj.gameObject);
                roleDict.Remove(playSn);
            }
        }

        public void ParseSyncEntityStatus(Proto.SyncEntityStatus proto)
        {
            if (npcDict.ContainsKey(proto.Sn) && npcDict[proto.Sn] != null)
                npcDict[proto.Sn].ParseStatus(proto);
            else if (roleDict.ContainsKey(proto.Sn))
                roleDict[proto.Sn].obj.ParseStatus(proto);
            else if (TeamManager.Instance.teamDict.ContainsKey(proto.Sn))
                TeamManager.Instance.teamDict[proto.Sn].UpdateHp(proto.Hp);
        }

        public void ParseSyncPlayerCmd(Proto.SyncPlayerCmd proto)
        {
            if (roleDict.ContainsKey(proto.PlayerSn))
            {
                Proto.AppearRole player = roleDict[proto.PlayerSn];
                if (player.obj != null)
                    player.obj.ParseCmd(proto);
            }
        }

        public void ParseReqSyncNpc(Proto.ReqSyncNpc proto)
        {
            if (_npcs.Count > 0)
            {
                _npcs[proto.NpcId].Sn = proto.NpcSn;
                if (_npcs[proto.NpcId] != null)
                    npcDict.TryAdd(proto.NpcSn, _npcs[proto.NpcId]);
            }
        }

        public void ParseSyncNpcPos(Proto.SyncNpcPos proto)
        {
            if (npcDict.ContainsKey(proto.NpcSn) && npcDict[proto.NpcSn] != null)
                npcDict[proto.NpcSn].ParsePos(proto.Pos);
        }

        public void ParseSyncFsmState(Proto.SyncFsmState proto)
        {
            PlayerController target = null;
            if (roleDict.ContainsKey(proto.PlayerSn))
                target = roleDict[proto.PlayerSn].obj;
            //if (npcDict.ContainsKey(proto.NpcSn) && npcDict[proto.NpcSn] != null)
            //    npcDict[proto.NpcSn].ParseFsmState((StateType)proto.State, proto.Code, target);
        }

        public void ParseReqLinkPlayer(Proto.ReqLinkPlayer proto)
        {
            if (!npcDict.ContainsKey(proto.NpcSn) && _npcs.Count > 0)
                npcDict.TryAdd(proto.NpcSn, _npcs[proto.NpcId]);
            if (npcDict.ContainsKey(proto.NpcSn))
                npcDict[proto.NpcSn].LinkPlayer(proto.Linker);
        }

        public void ParseDropItemList(Proto.DropItemList proto)
        {
            if (npcDict.ContainsKey(proto.NpcSn) && npcDict[proto.NpcSn] != null)
                npcDict[proto.NpcSn].DropItems(proto);
        }

        public void ParseSyncBtAction(Proto.SyncBtAction proto)
        {
            if (npcDict.ContainsKey(proto.NpcSn) && npcDict[proto.NpcSn] != null)
            {
                BtController npc = npcDict[proto.NpcSn];
                npc.Target = roleDict.ContainsKey(proto.PlayerSn) ? roleDict[proto.PlayerSn].obj.transform : null;
                if ((BtEventId)proto.Id == BtEventId.Patrol)
                    npc.patrolPath.index = proto.Code;
                npc.root?.SyncAction(proto.Id);
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