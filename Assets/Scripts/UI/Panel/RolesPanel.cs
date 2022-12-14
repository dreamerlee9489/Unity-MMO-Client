using Manage;
using Google.Protobuf;
using Net;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RolesPanel : BasePanel
    {
        private Button _playBtn = null, _createBtn = null;
        private string _token, _account;
        public ScrollRect _rolesRect { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _panelType = PanelType.RolesPanel;
            _rolesRect = transform.Find("RolesRect").GetComponent<ScrollRect>();
            _playBtn = transform.Find("PlayBtn").GetComponent<Button>();
            _createBtn = transform.Find("CreateBtn").GetComponent<Button>();

            _rolesRect.content.GetComponent<ToggleGroup>().allowSwitchOff = false;
            _createBtn.onClick.AddListener(() =>
            {
                _canvas.GetPanel<CreatePanel>().Open();
                Close();
            });
            _playBtn.onClick.AddListener(() =>
            {
                for (int i = 0; i < _rolesRect.content.childCount; ++i)
                {
                    if (_rolesRect.content.GetChild(i).GetComponent<Toggle>().isOn)
                    {
                        Proto.SelectPlayer proto = new Proto.SelectPlayer();
                        proto.PlayerSn = _rolesRect.content.GetChild(i).GetComponent<RoleToggle>().Id;
                        NetManager.Instance.SendPacket(Proto.MsgId.C2LSelectPlayer, proto);
                        break;
                    }
                }
            });

            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.L2CGameToken, GameTokenHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2GLoginByTokenRs, LoginByTokenRsHandler);
            EventManager.Instance.AddListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        protected override void Start()
        {
            base.Start();
            Close();
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.L2CGameToken, GameTokenHandler);
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2GLoginByTokenRs, GameTokenHandler);
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        public override void Close()
        {
            int childCount = _rolesRect.content.childCount;
            for (int i = 0; i < childCount; ++i)
                PoolManager.Instance.Push(PoolType.RoleToggle, _rolesRect.content.GetChild(0).gameObject);
            base.Close();
        }

        private void GameTokenHandler(IMessage msg)
        {
            Proto.GameToken proto = msg as Proto.GameToken;
            if (proto != null && NetManager.Instance.AppType == EAppType.Login)
            {
                //ModalPanel panel = GameManager.Instance.Canvas.GetPanel<ModalPanel>();
                //panel.Title.text = "登录消息";
                //panel.Msg.text = "正在连接游戏服务器";
                //panel.Open();
                _token = proto.Token;
                _account = GameManager.Instance.AccountInfo.Account;
                //print("RolesPanel.GameTokenHandler token=" + _token + " account=" + _account + " apptype=" + NetManager.Instance.AppType);
                NetManager.Instance.Disconnect();
                NetManager.Instance.Connect(proto.Ip, proto.Port, EAppType.Game);
            }
        }

        private void LoginByTokenRsHandler(IMessage msg)
        {
            Proto.LoginByTokenRs proto = msg as Proto.LoginByTokenRs;
            if (proto == null)
                return;
            //print("RolesPanel.LoginByTokenRsHandler code: " + proto.ReturnCode);
            switch (proto.ReturnCode)
            {
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcNotFoundAccount:
                    break;
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcTokenWrong:
                    break;
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcUnkonwn:
                    break;
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcOk:
                    break;
                default:
                    break;
            }
        }

        private void ConnectedCallback(EAppType appType)
        {
            if (appType == EAppType.Game)
                MonoManager.Instance.StartCoroutine(SendTokenDelay());
        }

        private void DisconnectCallback(EAppType appType)
        {
            //print("RolesPanel.DisconnectCallback apptype=" + appType);
        }

        private IEnumerator SendTokenDelay()
        {
            yield return new WaitForSeconds(1);
            Proto.LoginByToken proto = new Proto.LoginByToken
            {
                Token = _token,
                Account = _account
            };
            NetManager.Instance.SendPacket(Proto.MsgId.C2GLoginByToken, proto);
        }
    }
}
