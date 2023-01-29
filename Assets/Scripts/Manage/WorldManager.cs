﻿using Control;
using Control.BT;
using Items;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Manage
{
    public class WorldManager : MonoBehaviour
    {
        private readonly List<BtController> _npcs = new();

        public string fileName = "";
        public Dictionary<ulong, GameItem> itemDict = new();
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
                    var npcObj = Instantiate(obj).GetComponent<BtController>();
                    npcObj.gameObject.SetActive(false);
                    npcObj.id = id++;
                    npcObj.lv = int.Parse(strs[2]);
                    npcObj.hp = int.Parse(strs[3]);
                    npcObj.atk = int.Parse(strs[4]);
                    npcObj.initPos = npcObj.transform.position = pos.Parse(strs[5]);
                    npcObj.initHp = npcObj.hp;
                    npcObj.SetNameBar(strs[6]);
                    npcObj.patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
                    npcObj.patrolPath.transform.position = npcObj.transform.position;
                    npcObj.gameObject.SetActive(true);
                    _npcs.Add(npcObj);
                    Proto.ReqNpcInfo proto = new()
                    {
                        NpcId = npcObj.id,
                        NpcSn = 0
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SReqNpcInfo, proto);
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
                Destroy(roleDict[playSn].obj.gameObject);
                roleDict.Remove(playSn);
            }
        }

        public void ParseSyncEntityStatus(Proto.SyncEntityStatus proto)
        {
            if (npcDict.ContainsKey(proto.Sn))
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

        public void ParseReqNpcInfo(Proto.ReqNpcInfo proto)
        {
            var npc = _npcs[proto.NpcId];
            npc.Sn = proto.NpcSn;
            npc.gameObject.SetActive(false);
            npc.transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            npcDict.TryAdd(proto.NpcSn, npc);
            npc.gameObject.SetActive(true);
        }

        public void ParseSyncFsmState(Proto.SyncFsmState proto)
        {
            PlayerController target = null;
            if (roleDict.ContainsKey(proto.PlayerSn))
                target = roleDict[proto.PlayerSn].obj;
            //if (npcDict.ContainsKey(proto.NpcSn))
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
            if (npcDict.ContainsKey(proto.NpcSn))
                npcDict[proto.NpcSn].DropItems(proto);
        }

        public void ParseSyncBtAction(Proto.SyncBtAction proto)
        {
            if (npcDict.ContainsKey(proto.NpcSn))
            {
                BtController npc = npcDict[proto.NpcSn];
                if ((BtEventId)proto.Id == BtEventId.Patrol)
                    npc.patrolPath.index = proto.Code;
                npc.Target = roleDict.ContainsKey(proto.PlayerSn) ? roleDict[proto.PlayerSn].obj.transform : null;
                if (proto.PlayerSn != 0 && npc.Target == null)
                    MonoManager.Instance.StartCoroutine(WaitPlayerLoaded(proto));
                else
                    npc.root.SyncAction((BtEventId)proto.Id);
            }
        }

        private IEnumerator WaitPlayerLoaded(Proto.SyncBtAction proto)
        {
            yield return new WaitForSeconds(1.5f);
            npcDict[proto.NpcSn].Target = roleDict[proto.PlayerSn].obj.transform;
            npcDict[proto.NpcSn].root.SyncAction((BtEventId)proto.Id);
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