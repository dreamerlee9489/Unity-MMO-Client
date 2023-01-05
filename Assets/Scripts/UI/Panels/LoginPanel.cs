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
                Proto.AccountCheck proto = new()
                {
                    Account = _username.text,
                    Password = NetManager.Instance.Md5(System.Text.Encoding.Default.GetBytes(_password.text))
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2LAccountCheck, proto);
                Close();
            });

            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.C2LAccountCheckRs, AccountCheckRsHandler);
            EventManager.Instance.AddListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.AddListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
            Close();
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.C2LAccountCheckRs, AccountCheckRsHandler);
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Disconnect, DisconnectCallback);
        }

        private void AccountCheckRsHandler(Google.Protobuf.IMessage msg)
        {
            if (msg is Proto.AccountCheckRs proto)
            {
                Proto.AccountCheckReturnCode code = proto.ReturnCode;
                switch (code)
                {
                    case Proto.AccountCheckReturnCode.ArcOk:
                        break;
                    case Proto.AccountCheckReturnCode.ArcUnkonwn:
                        break;
                    case Proto.AccountCheckReturnCode.ArcNotFoundAccount:
                        break;
                    case Proto.AccountCheckReturnCode.ArcPasswordWrong:
                        break;
                    case Proto.AccountCheckReturnCode.ArcLogging:
                        break;
                    case Proto.AccountCheckReturnCode.ArcTimeout:
                        break;
                    case Proto.AccountCheckReturnCode.ArcOnline:
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
