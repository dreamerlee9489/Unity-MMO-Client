using Cinemachine;
using Items;
using System;
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

    public class GameManager : MonoSingleton<GameManager>
    {
        private UIManager _canvas;
        private Proto.AccountInfo _accountInfo;
        private Proto.PlayerInfo _player;
        private CinemachineVirtualCamera _virtualCam;
        private WorldManager _currWorld;
        private List<PlayerBaseData> _playerBaseDatas;
        private Dictionary<string, List<string>> _dropPotionDict, _dropWeaponDict;

        public UIManager Canvas => _canvas;
        public Proto.AccountInfo AccountInfo => _accountInfo;
        public Proto.PlayerInfo MainPlayer => _player;
        public CinemachineVirtualCamera VirtualCam => _virtualCam;
        public WorldManager CurrWorld => _currWorld;

        public List<PlayerBaseData> PlayerBaseDatas => _playerBaseDatas;
        public Dictionary<string, List<string>> DropPotionDict => _dropPotionDict;
        public Dictionary<string, List<string>> DropWeaponDict => _dropWeaponDict;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GameObject.Find("UIManager").GetComponent<UIManager>();
            _virtualCam = transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
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
            _currWorld = null;
            MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeAlpha());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _currWorld = FindObjectOfType<WorldManager>();
            MainPlayer.Obj.currWorld = _currWorld;
        }

        private void SyncPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncPlayer proto)
            {
                _player ??= new Proto.PlayerInfo();
                _player.LoadMainPlayer(proto.Player);
            }
        }

        private void PlayerListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerList proto)
            {
                _accountInfo ??= new Proto.AccountInfo();
                _accountInfo.ParseProto(proto);
                if (_accountInfo.Players.Count == 0)
                    _canvas.GetPanel<CreatePanel>().Open();
                else
                {
                    Transform content = _canvas.GetPanel<RolesPanel>().RolesRect.content;
                    for (int i = 0; i < _accountInfo.Players.Count; i++)
                    {
                        RoleToggle roleToggle = PoolManager.Instance.Pop(PoolType.RoleToggle, content).GetComponent<RoleToggle>();
                        roleToggle.Name.text = _accountInfo.Players[i].Name;
                        roleToggle.Level.text = "Lv " + _accountInfo.Players[i].Level;
                        roleToggle.Id = _accountInfo.Players[i].Id;
                    }
                    _canvas.GetPanel<RolesPanel>().Open();
                }
            }
        }

        private void EnterWorldHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.EnterWorld proto && proto.WorldId > 2)
            {
                _canvas.GetPanel<StartPanel>().Close();
                SceneManager.LoadSceneAsync(proto.WorldId - 2, LoadSceneMode.Single);
                MainPlayer.Obj.transform.position = new(proto.Position.X, proto.Position.Y, proto.Position.Z);
            }
        }

        private void AllRoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.AllRoleAppear proto)
                _currWorld.ParseAllRoleAppear(proto);
        }

        private void RoleDisappearHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.RoleDisappear proto)
                _currWorld.ParseRoleDisappear(proto);
        }

        private void JoinTeamResHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.JoinTeamRes proto)
                _currWorld.ParseJoinTeamRes(proto);
        }

        private void ReqJoinTeamHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqJoinTeam proto)
                _currWorld.ParseReqJoinTeam(proto);
        }

        private void PlayerKnapHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.PlayerKnap proto)
                MainPlayer.Obj.ParsePlayerKnap(proto);
        }

        private void DropItemListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.DropItemList proto)
                _currWorld.ParseDropItemList(proto);
        }

        private void ReqLinkPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqLinkPlayer proto)
                _currWorld.ParseReqLinkPlayer(proto);
        }

        private void SyncPlayerCmdHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncPlayerCmd proto)
                _currWorld.ParseSyncPlayerCmd(proto);
        }

        private void SyncFsmStateHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncFsmState proto)
                _currWorld.ParseSyncFsmState(proto);
        }

        private void SyncNpcPosHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncNpcPos proto)
                _currWorld.ParseSyncNpcPos(proto);
        }

        private void ReqSyncNpcHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.ReqSyncNpc proto)
                _currWorld.ParseReqSyncNpc(proto);
        }

        private void SyncEntityStatusHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.SyncEntityStatus proto)
                _currWorld.ParseSyncEntityStatus(proto);
        }

        private void ParsePlayerBaseCsv()
        {
            using var reader = File.OpenText($"{Application.streamingAssetsPath}/CSV/player_base.csv");
            reader.ReadLine();
            _playerBaseDatas = new() { new PlayerBaseData() };
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
                _playerBaseDatas.Add(baseData);
            }
        }

        private void ParseItemPotionsCsv()
        {
            using var reader = File.OpenText($"{Application.streamingAssetsPath}/CSV/ItemPotions.csv");
            reader.ReadLine();
            string line;
            _dropPotionDict = new();
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                _dropPotionDict.Add((int)ItemType.Potion + "@" + strs[0], new List<string>() { strs[1], strs[2] });
            }
        }

        private void ParseItemWeaponsCsv()
        {
            using var reader = File.OpenText($"{Application.streamingAssetsPath}/CSV/ItemWeapons.csv");
            reader.ReadLine();
            string line;
            _dropWeaponDict = new();
            while ((line = reader.ReadLine()) != null)
            {
                string[] strs = line.Split(',');
                _dropWeaponDict.Add((int)ItemType.Weapon + "@" + strs[0], new List<string>() { strs[1], strs[2] });
            }
        }
    }
}
