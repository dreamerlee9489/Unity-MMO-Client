using Cinemachine;
using LitJson;
using Net;
using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Manage
{
    public class GameManager : MonoSingleton<GameManager>
    {
        private UIManager _canvas;
        private AccountInfo _accountInfo;
        private PlayerInfo _mainPlayer;
        private CinemachineVirtualCamera _virtualCam;
        private WorldManager _activeWorld;

        public UIManager Canvas => _canvas;
        public AccountInfo AccountInfo => _accountInfo;
        public PlayerInfo MainPlayer => _mainPlayer;
        public CinemachineVirtualCamera VirtualCam => _virtualCam;
        public WorldManager ActiveWorld => _activeWorld;

        public string serverIp;
        public int serverPort;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GameObject.Find("UIManager").GetComponent<UIManager>();
            _virtualCam = transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
            MsgManager.Instance.RegistMsgHandler(MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(MsgId.S2CEnterWorld, EnterWorldHandler);
            EventManager.Instance.AddListener<bool>(EEventType.HotUpdated, HotUpdatedCallback);
            PoolManager.Instance.Add(PoolType.RoleToggle, ResourceManager.Instance.Load<GameObject>("UI/RoleToggle"));
            PoolManager.Instance.Add(PoolType.PatrolPath, ResourceManager.Instance.Load<GameObject>("Entity/Enemy/PatrolPath"));
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            MsgManager.Instance.RemoveMsgHandler(MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(MsgId.S2CEnterWorld, EnterWorldHandler);
            EventManager.Instance.RemoveListener<bool>(EEventType.HotUpdated, HotUpdatedCallback);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _activeWorld = FindObjectOfType<WorldManager>();
        }

        private IEnumerator ConnectServer()
        {
            UnityWebRequest request = UnityWebRequest.Get($"http://{serverIp}:{serverPort}/login");
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log("ConnectServer error: " + request.error);
            else
            {
                string result = request.downloadHandler.text;
                HttpJson data = JsonMapper.ToObject<HttpJson>(result);
                if (data.returncode == 0)
                    NetManager.Instance.Connect(data.ip, data.port, EAppType.Login);
                else
                    Debug.Log("ConnectServer error: " + request.error);
            }
        }

        private void PlayerListHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is PlayerList proto)
            {
                _accountInfo ??= new AccountInfo();
                _accountInfo.ParseProto(proto);
                if (_accountInfo.Players.Count == 0)
                    Canvas.GetPanel<CreatePanel>().Open();
                else
                {
                    Transform content = Canvas.GetPanel<RolesPanel>().RolesRect.content;
                    for (int i = 0; i < _accountInfo.Players.Count; i++)
                    {
                        RoleToggle roleToggle = PoolManager.Instance.Pop(PoolType.RoleToggle, content).GetComponent<RoleToggle>();
                        roleToggle.Name.text = _accountInfo.Players[i].Name;
                        roleToggle.Level.text = "Lv " + _accountInfo.Players[i].Level;
                        roleToggle.Id = _accountInfo.Players[i].Id;
                    }
                    Canvas.GetPanel<RolesPanel>().Open();
                }
            }
        }

        private void EnterWorldHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is EnterWorld proto && proto.WorldId > 2)
            {
                SceneManager.LoadSceneAsync(proto.WorldId - 2, LoadSceneMode.Single);
                Canvas.GetPanel<StartPanel>().Close();
            }
        }

        private void SyncPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is SyncPlayer proto)
            {
                _mainPlayer ??= new PlayerInfo();
                _mainPlayer.Parse(proto.Player);
            }
        }

        private void HotUpdatedCallback(bool updateOver)
        {
            UIManager.Instance.GetPanel<StartPanel>().Open();
            if (updateOver)
                MonoManager.Instance.StartCoroutine(ConnectServer());
            else
                UIManager.Instance.GetPanel<ModalPanel>().Open("检查更新", "更新失败！", ModalPanelType.Hint);
        }
    }
}
