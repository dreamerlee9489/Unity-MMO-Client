using Cinemachine;
using Items;
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
        private PlayerInfo _player;
        private CinemachineVirtualCamera _virtualCam;
        private WorldManager _activeWorld;
        private List<PlayerBaseData> _playerBaseDatas;
        private Dictionary<string, List<string>> _dropPotionDict, _dropWeaponDict;

        public UIManager Canvas => _canvas;
        public AccountInfo AccountInfo => _accountInfo;
        public PlayerInfo MainPlayer => _player;
        public CinemachineVirtualCamera VirtualCam => _virtualCam;
        public WorldManager CurrWorld => _activeWorld;

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
            MsgManager.Instance.RegistMsgHandler(MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CEnterWorld, EnterWorldHandler);
            PoolManager.Instance.Inject(PoolType.RoleToggle, "UI/RoleToggle", 20);
            PoolManager.Instance.Inject(PoolType.PatrolPath, "Entity/NPC/PatrolPath");
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
            _activeWorld = null;
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
                _canvas.FindPanel<StartPanel>().Close();
                SceneManager.LoadSceneAsync(proto.WorldId - 2, LoadSceneMode.Single);
                MainPlayer.Obj.transform.position = new(proto.Position.X, proto.Position.Y, proto.Position.Z);
            }
        }

        private void SyncPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is SyncPlayer proto)
            {
                _player ??= new PlayerInfo();
                _player.LoadPlayer(proto.Player);
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
