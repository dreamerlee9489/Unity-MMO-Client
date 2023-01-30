﻿using Cinemachine;
using Items;
using System.Collections.Generic;
using System.IO;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manage
{
    public class PlayerBaseData
    {
        public int xp, hp, mp, atk, def;

        public PlayerBaseData() { }

        public PlayerBaseData(int xp, int hp, int mp, int atk, int def)
        {
            this.xp = xp;
            this.hp = hp;
            this.mp = mp;
            this.atk = atk;
            this.def = def;
        }
    }

    public class GameManager : MonoBehaviour
    {
        public UIManager canvas;
        public Proto.AccountInfo accountInfo;
        public Proto.PlayerInfo mainPlayer;
        public CinemachineVirtualCamera virtualCam;
        public List<PlayerBaseData> playerBaseDatas;
        public Dictionary<int, string> worldDict;
        public Dictionary<string, List<string>> dropPotionDict;
        public Dictionary<string, List<string>> dropWeaponDict;
        public static WorldManager currWorld;

        private static GameManager _instance;
        public static GameManager Instance => _instance;

        protected void Awake()
        {
            _instance = this;
            canvas = GameObject.Find("UIManager").GetComponent<UIManager>();
            virtualCam = transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
            DontDestroyOnLoad(gameObject);
            ParsePlayerBaseCsv();
            ParseItemPotionsCsv();
            ParseItemWeaponsCsv();
            ParseWorldCsv();
            PoolManager.Instance.Load(PoolType.RoleToggle, "UI/RoleToggle", 20);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnterWorld, EnterWorldHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CAllRoleAppear, AllRoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncEntityStatus, SyncEntityStatusHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CReqNpcInfo, ReqNpcInfoHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncFsmState, SyncFsmStateHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncPlayerCmd, SyncPlayerCmdHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CReqLinkPlayer, ReqLinkPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CDropItemList, DropItemListHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CGetPlayerKnap, PlayerKnapHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.MiGlobalChat, GlobalChatHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.MiWorldChat, WorldChatHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.MiTeamChat, TeamChatHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.MiPrivateChat, PrivateChatHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.MiCreateTeam, CreateTeamHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CReqJoinTeam, ReqJoinTeamHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CJoinTeamRes, JoinTeamResHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CReqEnterDungeon, ReqEnterDungeonHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CReqPvp, ReqPvpHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CPvpRes, PvpResHandler);            
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CReqTrade, ReqTradeHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2CUpdateTradeItem, UpdateTradeItemHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CTradeOpen, TradeOpenHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CTradeClose, TradeCloseHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CSyncBtAction, SyncBtActionHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CPlayerMove, PlayerMoveHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CNpcMove, NpcMoveHandler);
        }

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            MonoManager.Instance.StopAllCoroutines();
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnterWorld, EnterWorldHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CAllRoleAppear, AllRoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleDisappear, RoleDisappearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncEntityStatus, SyncEntityStatusHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CReqNpcInfo, ReqNpcInfoHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncFsmState, SyncFsmStateHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncPlayerCmd, SyncPlayerCmdHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CReqLinkPlayer, ReqLinkPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CDropItemList, DropItemListHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CGetPlayerKnap, PlayerKnapHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.MiGlobalChat, GlobalChatHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.MiWorldChat, WorldChatHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.MiTeamChat, TeamChatHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.MiPrivateChat, PrivateChatHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.MiCreateTeam, CreateTeamHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CReqJoinTeam, ReqJoinTeamHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CJoinTeamRes, JoinTeamResHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CReqEnterDungeon, ReqEnterDungeonHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CReqPvp, ReqPvpHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CPvpRes, PvpResHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CReqTrade, ReqTradeHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2CUpdateTradeItem, UpdateTradeItemHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CTradeOpen, TradeOpenHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CTradeClose, TradeCloseHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CSyncBtAction, SyncBtActionHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CPlayerMove, PlayerMoveHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CNpcMove, NpcMoveHandler);
        }

        private void ParsePlayerBaseCsv()
        {
            using var reader = File.OpenText($"{Application.streamingAssetsPath}/CSV/player_base.csv");
            reader.ReadLine();
            playerBaseDatas = new() { new PlayerBaseData() };
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                PlayerBaseData baseData = new()
                {
                    xp = int.Parse(strs[1]),
                    hp = int.Parse(strs[2]),
                    mp = int.Parse(strs[3]),
                    atk = int.Parse(strs[4]),
                    def = int.Parse(strs[5])
                };
                playerBaseDatas.Add(baseData);
            }
        }

        private void ParseItemPotionsCsv()
        {
            using var reader = File.OpenText($"{Application.streamingAssetsPath}/CSV/ItemPotions.csv");
            reader.ReadLine();
            string line;
            dropPotionDict = new();
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                dropPotionDict.Add((int)ItemType.Potion + "@" + strs[0], new List<string>() { strs[1], strs[2] });
            }
        }

        private void ParseItemWeaponsCsv()
        {
            using var reader = File.OpenText($"{Application.streamingAssetsPath}/CSV/ItemWeapons.csv");
            reader.ReadLine();
            string line;
            dropWeaponDict = new();
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                dropWeaponDict.Add((int)ItemType.Weapon + "@" + strs[0], new List<string>() { strs[1], strs[2] });
            }
        }

        private void ParseWorldCsv()
        {
            using var reader = File.OpenText($"{Application.streamingAssetsPath}/CSV/world.csv");
            reader.ReadLine();
            string line;
            worldDict = new();
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                worldDict.Add(int.Parse(strs[0]), strs[1]);
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            currWorld = null;
            mainPlayer.Obj.ResetCmd();
            canvas.GetPanel<ChatPanel>().Close();
            MonoManager.Instance.StartCoroutine(canvas.FadeAlpha());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currWorld = FindObjectOfType<WorldManager>();
            canvas.GetPanel<ChatPanel>().Open();
            canvas.WorldName.text = worldDict[scene.buildIndex + 2];
        }

        private void SyncPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncPlayer proto)
            {
                mainPlayer ??= new Proto.PlayerInfo();
                mainPlayer.LoadMainPlayer(proto.Player);
            }
        }

        private void PlayerListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerList proto)
            {
                accountInfo ??= new Proto.AccountInfo();
                accountInfo.ParseProto(proto);
                if (accountInfo.Roles.Count == 0)
                    canvas.GetPanel<CreatePanel>().Open();
                else
                {
                    Transform content = canvas.GetPanel<RolesPanel>().RolesRect.content;
                    for (int i = 0; i < accountInfo.Roles.Count; i++)
                    {
                        RoleToggle roleToggle = PoolManager.Instance.Pop(PoolType.RoleToggle, content).GetComponent<RoleToggle>();
                        roleToggle.Name.text = accountInfo.Roles[i].Name;
                        roleToggle.Level.text = "Lv " + accountInfo.Roles[i].Level;
                        roleToggle.Id = accountInfo.Roles[i].Id;
                    }
                    canvas.GetPanel<RolesPanel>().Open();
                }
            }
        }

        private void EnterWorldHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.EnterWorld proto && proto.WorldId > 2)
            {
                canvas.GetPanel<StartPanel>().Close();
                SceneManager.LoadSceneAsync(proto.WorldId - 2, LoadSceneMode.Single);
                mainPlayer.Obj.transform.position = new(proto.Position.X, proto.Position.Y, proto.Position.Z);
            }
        }

        private void AllRoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.AllRoleAppear proto && currWorld)
                currWorld.ParseAllRoleAppear(proto);
        }

        private void RoleDisappearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RoleDisappear proto && currWorld)
                currWorld.ParseRoleDisappear(proto);
        }

        private void DropItemListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.DropItemList proto && currWorld)
                currWorld.ParseDropItemList(proto);
        }

        private void ReqLinkPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqLinkPlayer proto && currWorld)
                currWorld.ParseReqLinkPlayer(proto);
        }

        private void SyncPlayerCmdHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncPlayerCmd proto && currWorld)
                currWorld.ParseSyncPlayerCmd(proto);
        }

        private void SyncFsmStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncFsmState proto && currWorld)
                currWorld.ParseSyncFsmState(proto);
        }

        private void ReqNpcInfoHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqNpcInfo proto && currWorld)
                currWorld.ParseReqNpcInfo(proto);
        }

        private void SyncEntityStatusHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncEntityStatus proto && currWorld)
                currWorld.ParseSyncEntityStatus(proto);
        }

        private void SyncBtActionHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncBtAction proto && currWorld)
                MonoManager.Instance.StartCoroutine(currWorld.ParseSyncBtAction(proto));
        }

        private void NpcMoveHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.EntityMove proto && currWorld)
                MonoManager.Instance.StartCoroutine(currWorld.ParseNpcMove(proto));
        }

        private void PlayerMoveHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.EntityMove proto && currWorld)
                MonoManager.Instance.StartCoroutine(currWorld.ParsePlayerMove(proto));
        }

        private void PlayerKnapHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerKnap proto)
                mainPlayer.Obj.ParsePlayerKnap(proto);
        }

        private void GlobalChatHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is Proto.ChatMsg proto)
                canvas.GetPanel<ChatPanel>().ShowMsg(ChatType.Global, $"{proto.Name}：{proto.Content}");
        }

        private void WorldChatHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ChatMsg proto)
                canvas.GetPanel<ChatPanel>().ShowMsg(ChatType.World, $"{proto.Name}：{proto.Content}");
        }

        private void TeamChatHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is Proto.ChatMsg proto)
                canvas.GetPanel<ChatPanel>().ShowMsg(ChatType.Team, $"{proto.Name}：{proto.Content}");
        }

        private void PrivateChatHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ChatMsg proto)
                canvas.GetPanel<ChatPanel>().ShowMsg(ChatType.Private, $"{proto.Name}：{proto.Content[(proto.Content.IndexOf(' ') + 1)..]}");
        }

        private void ReqJoinTeamHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerReq proto)
                TeamManager.Instance.ParseReqJoinTeam(proto);
        }

        private void JoinTeamResHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerReq proto)
                TeamManager.Instance.ParseJoinTeamRes(proto);
        }

        private void CreateTeamHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is Proto.CreateTeam proto)
                TeamManager.Instance.ParseCreateTeam(proto);
        }

        private void ReqEnterDungeonHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is Proto.EnterDungeon proto)
            {
                string text = $"玩家[{proto.Sender}]邀请你进入副本[{worldDict[proto.WorldId]}]，是否同意？";
                canvas.GetPanel<PopupPanel>().Open(text, () =>
                {
                    Proto.EnterDungeon protoRes = new()
                    {
                        WorldId = proto.WorldId,
                        WorldSn = proto.WorldSn,
                        Agree = true
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2CEnterDungeonRes, protoRes);
                }, null);
            }
        }

        private void ReqPvpHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerReq proto)
            {
                string text = $"玩家[{currWorld.roleDict[proto.Applicant].name}]向你发起挑战，是否同意？";
                canvas.GetPanel<PopupPanel>().Open(text, () =>
                {
                    Proto.PlayerReq protoRes = new()
                    {
                        Applicant = proto.Applicant,
                        Responder = mainPlayer.Sn,
                        Agree = true
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2CPvpRes, protoRes);
                }, null);
            }
        }

        private void PvpResHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerReq proto)
            {
                if (mainPlayer.Sn == proto.Applicant)
                {
                    var defer = currWorld.roleDict[proto.Responder].obj;
                    defer.tag = "Enemy";
                    defer.NameBar.ChangeCamp();
                }
                else if (mainPlayer.Sn == proto.Responder)
                {
                    var atker = currWorld.roleDict[proto.Applicant].obj;
                    atker.tag = "Enemy";
                    atker.NameBar.ChangeCamp();
                }
            }
        }

        private void ReqTradeHandler(Google.Protobuf.IMessage msg)
        {
            if(msg is Proto.PlayerReq proto) 
            {
                string text = $"玩家[{currWorld.roleDict[proto.Applicant].name}]邀请你交易，是否同意？";
                canvas.GetPanel<PopupPanel>().Open(text, () =>
                {
                    Proto.PlayerReq protoRes = new()
                    {
                        Applicant = proto.Applicant,
                        Responder = mainPlayer.Sn,
                        Agree = true
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2CTradeRes, protoRes);
                    canvas.GetPanel<TradePanel>().Open(currWorld.roleDict[proto.Applicant], currWorld.roleDict[mainPlayer.Sn]);
                }, null);
            }
        }

        private void UpdateTradeItemHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.UpdateTradeItem proto)
            {
                if (proto.Item != null)
                    mainPlayer.Obj.ParseItemToKnap(proto.Item);
                if (proto.Gold > 0)
                    canvas.GetPanel<TradePanel>().RemoteRect.GoldField.text = proto.Gold.ToString();
                canvas.GetPanel<TradePanel>().RemoteRect.SureTog.isOn = proto.Ack;
            }
        }

        private void TradeOpenHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.TradeOpen proto)
                canvas.GetPanel<TradePanel>().Open(currWorld.roleDict[proto.Applicant], currWorld.roleDict[proto.Responder]);
        }

        private void TradeCloseHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.TradeClose proto)
            {
                canvas.GetPanel<TradePanel>().Close();
                mainPlayer.Obj.ParseTradeClose(proto);
            }
        }
    }
}
