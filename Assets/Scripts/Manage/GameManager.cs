﻿using Cinemachine;
using Item;
using Proto;
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
        private AccountInfo _accountInfo;
        private PlayerInfo _mainPlayer;
        private CinemachineVirtualCamera _virtualCam;
        private WorldManager _activeWorld;
        private List<PlayerBaseData> _playerBaseDatas;
        private Dictionary<int, string> _dropPotionDict, _dropWeaponDict;

        public UIManager Canvas => _canvas;
        public AccountInfo AccountInfo => _accountInfo;
        public PlayerInfo MainPlayer => _mainPlayer;
        public CinemachineVirtualCamera VirtualCam => _virtualCam;
        public WorldManager ActiveWorld => _activeWorld;

        public List<PlayerBaseData> PlayerBaseDatas => _playerBaseDatas;
        public Dictionary<int, string> DropPotionDict => _dropPotionDict;
        public Dictionary<int, string> DropWeaponDict => _dropWeaponDict;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GameObject.Find("UIManager").GetComponent<UIManager>();
            _virtualCam = transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
            ParsePlayerBaseCsv();
            ParseItemPotionsCsv();
            ParseItemWeaponsCsv();
            MsgManager.Instance.RegistMsgHandler(MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CEnterWorld, EnterWorldHandler);
            PoolManager.Instance.Add(PoolType.RoleToggle, ResourceManager.Instance.Load<GameObject>("UI/RoleToggle"));
            PoolManager.Instance.Add(PoolType.PatrolPath, ResourceManager.Instance.Load<GameObject>("Entity/Enemy/PatrolPath"));
        }

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            MsgManager.Instance.RemoveMsgHandler(MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CEnterWorld, EnterWorldHandler);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _canvas.FindPanel<HUDPanel>().Close();
            MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeAlpha());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _activeWorld = FindObjectOfType<WorldManager>();
            _canvas.FindPanel<HUDPanel>().Open();
        }       

        private void PlayerListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is PlayerList proto)
            {
                _accountInfo ??= new AccountInfo();
                _accountInfo.ParseProto(proto);
                if (_accountInfo.Players.Count == 0)
                    Canvas.FindPanel<CreatePanel>().Open();
                else
                {
                    Transform content = Canvas.FindPanel<RolesPanel>().RolesRect.content;
                    for (int i = 0; i < _accountInfo.Players.Count; i++)
                    {
                        RoleToggle roleToggle = PoolManager.Instance.Pop(PoolType.RoleToggle, content).GetComponent<RoleToggle>();
                        roleToggle.Name.text = _accountInfo.Players[i].Name;
                        roleToggle.Level.text = "Lv " + _accountInfo.Players[i].Level;
                        roleToggle.Id = _accountInfo.Players[i].Id;
                    }
                    Canvas.FindPanel<RolesPanel>().Open();
                }
            }
        }

        private void EnterWorldHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is EnterWorld proto && proto.WorldId > 2)
            {
                SceneManager.LoadSceneAsync(proto.WorldId - 2, LoadSceneMode.Single);
                _canvas.FindPanel<StartPanel>().Close();
            }
        }

        private void SyncPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is SyncPlayer proto)
            {
                _mainPlayer ??= new PlayerInfo();
                _mainPlayer.LoadPlayer(proto.Player);
            }
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
                _dropPotionDict.Add(int.Parse(strs[0]), strs[1]);
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
                _dropWeaponDict.Add(int.Parse(strs[0]), strs[1]);
            }
        }
    }
}
