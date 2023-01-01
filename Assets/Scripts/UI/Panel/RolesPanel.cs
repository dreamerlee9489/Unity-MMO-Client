using Manage;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RolesPanel : BasePanel
    {
        private string _token, _account;
        private Button _playBtn, _createBtn;
        public ScrollRect RolesRect { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _playBtn = transform.Find("PlayBtn").GetComponent<Button>();
            _createBtn = transform.Find("CreateBtn").GetComponent<Button>();
            RolesRect = transform.Find("RolesRect").GetComponent<ScrollRect>();

            RolesRect.content.GetComponent<ToggleGroup>().allowSwitchOff = false;
            _createBtn.onClick.AddListener(() =>
            {
                _canvas.FindPanel<CreatePanel>().Open();
                Close();
            });
            _playBtn.onClick.AddListener(() =>
            {
                for (int i = 0; i < RolesRect.content.childCount; ++i)
                {
                    if (RolesRect.content.GetChild(i).GetComponent<Toggle>().isOn)
                    {
                        Proto.SelectPlayer proto = new()
                        {
                            PlayerSn = RolesRect.content.GetChild(i).GetComponent<RoleToggle>().Id
                        };
                        NetManager.Instance.SendPacket(Proto.MsgId.C2LSelectPlayer, proto);
                        break;
                    }
                }
            });

            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.L2CGameToken, GameTokenHandler);
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2GLoginByTokenRs, LoginByTokenRsHandler);
            EventManager.Instance.AddListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
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
            int childCount = RolesRect.content.childCount;
            for (int i = 0; i < childCount; ++i)
                PoolManager.Instance.Push(PoolType.RoleToggle, RolesRect.content.GetChild(0).gameObject);
            base.Close();
        }

        private void GameTokenHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.GameToken proto && NetManager.Instance.AppType == EAppType.Login)
            {
                _token = proto.Token;
                _account = GameManager.Instance.AccountInfo.Account;
                NetManager.Instance.Disconnect();
                NetManager.Instance.Connect(proto.Ip, proto.Port, EAppType.Game);
            }
        }

        private void LoginByTokenRsHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.LoginByTokenRs proto)
            {
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
        }

        private void ConnectedCallback(EAppType appType)
        {
            if (appType == EAppType.Game)
                MonoManager.Instance.StartCoroutine(SendTokenDelay());
        }

        private void DisconnectCallback(EAppType appType)
        {
        }

        private IEnumerator SendTokenDelay()
        {
            yield return new WaitForSeconds(1);
            Proto.LoginByToken proto = new()
            {
                Token = _token,
                Account = _account
            };
            NetManager.Instance.SendPacket(Proto.MsgId.C2GLoginByToken, proto);
        }
    }
}
