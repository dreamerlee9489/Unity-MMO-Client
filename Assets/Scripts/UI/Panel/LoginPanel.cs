using Manage;
using UnityEngine.UI;

namespace UI
{
    public class LoginPanel : BasePanel
    {
        private InputField _username, _password;
        private Button _loginBtn;

        protected override void Awake()
        {
            base.Awake();
            _username = transform.Find("Username").GetComponent<InputField>();
            _password = transform.Find("Password").GetComponent<InputField>();
            _loginBtn = transform.Find("LoginBtn").GetComponent<Button>();

            _loginBtn.onClick.AddListener(() =>
            {
                Net.AccountCheck proto = new()
                {
                    Account = _username.text,
                    Password = NetManager.Instance.Md5(System.Text.Encoding.Default.GetBytes(_password.text))
                };
                NetManager.Instance.SendPacket(Net.MsgId.C2LAccountCheck, proto);
                Close();
            });

            MsgManager.Instance.RegistMsgHandler(Net.MsgId.C2LAccountCheckRs, AccountCheckRsHandler);
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
            MsgManager.Instance.RemoveMsgHandler(Net.MsgId.C2LAccountCheckRs, AccountCheckRsHandler);
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void AccountCheckRsHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Net.AccountCheckRs proto)
            {
                Net.AccountCheckReturnCode code = proto.ReturnCode;
                switch (code)
                {
                    case Net.AccountCheckReturnCode.ArcOk:
                        break;
                    case Net.AccountCheckReturnCode.ArcUnkonwn:
                        break;
                    case Net.AccountCheckReturnCode.ArcNotFoundAccount:
                        break;
                    case Net.AccountCheckReturnCode.ArcPasswordWrong:
                        break;
                    case Net.AccountCheckReturnCode.ArcLogging:
                        break;
                    case Net.AccountCheckReturnCode.ArcTimeout:
                        break;
                    case Net.AccountCheckReturnCode.ArcOnline:
                        break;
                    default:
                        break;
                }
            }
            Close();
        }

        private void ConnectedCallback(EAppType appType)
        {
            if (appType == EAppType.Login)
                Open();
        }

        private void DisconnectCallback(EAppType appType)
        {
        }
    }
}
