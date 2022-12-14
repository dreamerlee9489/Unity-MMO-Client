using Manage;
using Net;
using System;

namespace UI
{
    public class ModalPanel : BasePanel
    {
        private ModalBox _modalBox;

        protected override void Awake()
        {
            base.Awake();
            _panelType = PanelType.ModalPanel;
            _modalBox = transform.Find("ModalBox").GetComponent<ModalBox>();
            EventManager.Instance.AddListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.AddListener<EAppType>(EEventType.Connecting, ConnectingCallback);
        }

        protected override void Start()
        {
            base.Start();
            Close();
        }

        private void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Connected, ConnectedCallback);
            EventManager.Instance.RemoveListener<EAppType>(EEventType.Connecting, ConnectingCallback);
        }

        public void Open(string title, string msg)
        {
            _modalBox.Title.text = title;
            _modalBox.Msg.text = msg;
            base.Open();
        }

        private void ConnectingCallback(EAppType appType)
        {
            switch (appType)
            {
                case EAppType.Client:
                    break;
                case EAppType.Login:
                    Open("网络消息", "正在连接登录服务器...");
                    break;
                case EAppType.Game:
                    Open("网络消息", "正在连接游戏服务器...");
                    break;
                default:
                    break;
            }
        }

        private void ConnectedCallback(EAppType appType)
        {
            Close();
        }
    }
}
