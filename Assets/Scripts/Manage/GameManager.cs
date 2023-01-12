using Cinemachine;
using Control;
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
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnterWorld, EnterWorldHandler);
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
            PoolManager.Instance.LoadPush(PoolType.RoleToggle, "UI/RoleToggle", 20);
            PoolManager.Instance.LoadPush(PoolType.PatrolPath, "Entity/NPC/PatrolPath");
        }       

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnterWorld, EnterWorldHandler);
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

        private void OnSceneUnloaded(Scene scene)
        {
            currWorld = null;
            mainPlayer.Obj.ResetCmd();
            MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeAlpha());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currWorld = FindObjectOfType<WorldManager>();
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
                if (accountInfo.Players.Count == 0)
                    canvas.GetPanel<CreatePanel>().Open();
                else
                {
                    Transform content = canvas.GetPanel<RolesPanel>().RolesRect.content;
                    for (int i = 0; i < accountInfo.Players.Count; i++)
                    {
                        RoleToggle roleToggle = PoolManager.Instance.Pop(PoolType.RoleToggle, content).GetComponent<RoleToggle>();
                        roleToggle.Name.text = accountInfo.Players[i].Name;
                        roleToggle.Level.text = "Lv " + accountInfo.Players[i].Level;
                        roleToggle.Id = accountInfo.Players[i].Id;
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
            if (msg is Proto.AllRoleAppear proto)
                currWorld.ParseAllRoleAppear(proto);
        }

        private void RoleDisappearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RoleDisappear proto)
                currWorld.ParseRoleDisappear(proto);
        }

        private void JoinTeamResHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.JoinTeamRes proto)
                currWorld.ParseJoinTeamRes(proto);
        }

        private void ReqJoinTeamHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqJoinTeam proto)
                currWorld.ParseReqJoinTeam(proto);
        }

        private void PlayerKnapHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerKnap proto)
                mainPlayer.Obj.ParsePlayerKnap(proto);
        }

        private void DropItemListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.DropItemList proto)
                currWorld.ParseDropItemList(proto);
        }

        private void ReqLinkPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqLinkPlayer proto)
                currWorld.ParseReqLinkPlayer(proto);
        }

        private void SyncPlayerCmdHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncPlayerCmd proto)
                currWorld.ParseSyncPlayerCmd(proto);
        }

        private void SyncFsmStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncFsmState proto)
                currWorld.ParseSyncFsmState(proto);
        }

        private void SyncNpcPosHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncNpcPos proto)
                currWorld.ParseSyncNpcPos(proto);
        }

        private void ReqSyncNpcHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqSyncNpc proto)
                currWorld.ParseReqSyncNpc(proto);
        }

        private void SyncEntityStatusHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncEntityStatus proto)
                currWorld.ParseSyncEntityStatus(proto);
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
    }
}
