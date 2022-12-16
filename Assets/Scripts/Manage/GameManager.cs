using Net;
using LitJson;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UI;
using UnityEngine.SceneManagement;
using Cinemachine;

namespace Manage
{
    public class GameManager : MonoSingleton<GameManager>
    {
        private UICanvas _canvas;
        private AccountInfo _accountInfo;
        private PlayerInfo _mainPlayer;
        private CinemachineVirtualCamera _virtualCamera;
        private WorldManager _activeWorld;

        public string ip;
        public int port;

        public UICanvas Canvas => _canvas;
        public AccountInfo AccountInfo => _accountInfo;
        public PlayerInfo MainPlayer => _mainPlayer;
        public CinemachineVirtualCamera VirtualCamera => _virtualCamera;
        public WorldManager ActiveWorld => _activeWorld;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GameObject.Find("UICanvas").GetComponent<UICanvas>();
            _virtualCamera = transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
            MsgManager.Instance.RegistMsgHandler(Net.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RegistMsgHandler(Net.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(Net.MsgId.S2CEnterWorld, EnterWorldHandler);
            PoolManager.Instance.Add(PoolType.RoleToggle, ResourceManager.Instance.Load<GameObject>("UI/RoleToggle"));
            PoolManager.Instance.Add(PoolType.PatrolPath, ResourceManager.Instance.Load<GameObject>("Entity/Enemy/PatrolPath"));
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            MonoManager.Instance.StartCoroutine(ConnectServer());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            MsgManager.Instance.RemoveMsgHandler(Net.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RemoveMsgHandler(Net.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(Net.MsgId.S2CEnterWorld, EnterWorldHandler);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _activeWorld = FindObjectOfType<WorldManager>();
        }

        private IEnumerator ConnectServer()
        {
            UnityWebRequest request = UnityWebRequest.Get($"http://{ip}:{port}/login");
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
            if (msg is Net.PlayerList proto)
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
            if (msg is Net.EnterWorld proto && proto.WorldId > 2)
            {
                SceneManager.LoadSceneAsync(proto.WorldId - 2, LoadSceneMode.Single);
                Canvas.GetPanel<StartPanel>().Close();
            }
        }

        private void SyncPlayerHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Net.SyncPlayer proto)
            {
                _mainPlayer ??= new PlayerInfo();
                _mainPlayer.Parse(proto.Player);
            }
        }
    }
}
