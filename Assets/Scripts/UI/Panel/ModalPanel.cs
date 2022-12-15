using Manage;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ModalPanel : BasePanel
    {
        private Transform _modalBox;
        private Button _closeBtn;
        private Text _title;
        private Text _msg;

        protected override void Awake()
        {
            base.Awake();
            _panelType = PanelType.ModalPanel;
            _modalBox = transform.Find("ModalBox");
            _title = _modalBox.Find("Title").GetChild(0).GetComponent<Text>();
            _msg = _modalBox.Find("Msg").GetComponent<Text>();
            _closeBtn = _modalBox.Find("CloseBtn").GetComponent<Button>();
            _closeBtn.onClick.AddListener(Close);
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
            _title.text = title;
            _msg.text = msg;
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
