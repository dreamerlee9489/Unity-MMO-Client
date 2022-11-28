using Net;
using LitJson;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Collections.Generic;
using Control;

namespace Frame
{
    public class GameManager : MonoSingleton<GameManager>
    {
        private AccountInfo _accountInfo;
        private Player _mainPlayer;
        private UICanvas _canvas;
        private CinemachineVirtualCamera _virtualCamera;
        private Dictionary<ulong, AppearRole> _players = new();

        public string ip;
        public int port;

        public UICanvas Canvas => _canvas;
        public AccountInfo AccountInfo => _accountInfo;
        public Player MainPlayer => _mainPlayer;
        public CinemachineVirtualCamera VirtualCamera => _virtualCamera;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GameObject.Find("UICanvas").GetComponent<UICanvas>();
            _virtualCamera = transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnterWorld, EnterWorldHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CMove, SyncMoveHandler);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            MonoManager.Instance.StartCoroutine(ConnectServer());
            PoolManager.Instance.Add(PoolType.RoleToggle, ResourceManager.Instance.Load<GameObject>("UI/RoleToggle"), 10);
        }

        private void OnDestroy()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.L2CPlayerList, PlayerListHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.G2CSyncPlayer, SyncPlayerHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnterWorld, EnterWorldHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CRoleAppear, RoleAppearHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CMove, SyncMoveHandler);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        IEnumerator ConnectServer()
        {
            UnityWebRequest request = UnityWebRequest.Get($"http://{ip}:{port}/login");
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("ConnectServer error: " + request.error);
            }
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

        private void RoleAppearHandler(Google.Protobuf.IMessage msg)
        {
            print("GameManager.RoleAppearHandler");
            Proto.RoleAppear proto = msg as Proto.RoleAppear;
            if (proto != null)
            {
                foreach (Proto.Role role in proto.Role)
                {
                    ulong sn = role.Sn;
                    if (_players.ContainsKey(sn))
                        _players[sn].Parse(role);
                    else
                    {
                        AppearRole appearRole = new AppearRole();
                        appearRole.Parse(role);
                        appearRole.LoadObj();
                        _players.Add(sn, appearRole);
                        _players[sn].Obj.GetComponent<Entity>().NameBar.Name.text = role.Name;
                        Debug.Log("sync player sn=" + sn + " world =" + SceneManager.GetActiveScene().name);
                    }
                }
            }
        }

        private void PlayerListHandler(Google.Protobuf.IMessage msg)
        {
            Proto.PlayerList proto = msg as Proto.PlayerList;
            if (proto != null)
            {
                if (_accountInfo == null)
                    _accountInfo = new AccountInfo();
                _accountInfo.Parse(proto);
                if (_accountInfo.Players.Count == 0)
                {
                    Canvas.GetPanel<CreatePanel>().Open();
                }
                else
                {
                    RoleToggle roleToggle = null;
                    Transform content = Canvas.GetPanel<RolesPanel>()._rolesRect.content;
                    for (int i = 0; i < _accountInfo.Players.Count; i++)
                    {
                        roleToggle = PoolManager.Instance.Pop(PoolType.RoleToggle, content).GetComponent<RoleToggle>();
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
            Proto.EnterWorld proto = msg as Proto.EnterWorld;
            if (proto != null && proto.WorldId > 2)
            {
                //Debug.Log("GameManager.EnterWorldHandler id: " + proto.WorldId);
                SceneManager.LoadSceneAsync(proto.WorldId - 2, LoadSceneMode.Single);
                Canvas.GetPanel<StartPanel>().Close();
            }
        }

        private void SyncPlayerHandler(Google.Protobuf.IMessage msg)
        {
            Proto.SyncPlayer proto = msg as Proto.SyncPlayer;
            if (proto != null)
            {
                //print("GameManager.SyncPlayerHandler sn: " + proto.Player.Sn);
                if (_mainPlayer == null)
                    _mainPlayer = new Player();
                _mainPlayer.Parse(proto.Player);
            }
        }

        private void SyncMoveHandler(Google.Protobuf.IMessage msg)
        {
            Proto.Move proto = msg as Proto.Move;
            if (proto != null)
            {
                Debug.Log("SyncMoveHandler");
                ulong playSn = proto.PlayerSn;
                if (_players.ContainsKey(playSn))
                {
                    AppearRole player = _players[playSn];
                    Entity entity = player.Obj.GetComponent<Entity>();
                    entity.CornerPoints.AddRange(proto.Position);
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded " + scene.buildIndex + " mode=" + mode);
            EventManager.Instance.Invoke(EEventType.SceneLoaded, scene.buildIndex);
        }
    }
}
