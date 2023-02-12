using Manage;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum ModalPanelType { Notice, Hint }

    public class ModalPanel : BasePanel
    {
        private Text _hintTitle, _updateTitle;
        private Text _hintMsg, _updateMsg;
        private Button _hintCloseBtn, _updateCloseBtn;
        private RectTransform _hint, _update;

        protected override void Awake()
        {
            base.Awake();
            _hint = transform.Find("HintBox") as RectTransform;
            _hintTitle = _hint.Find("Title").GetChild(0).GetComponent<Text>();
            _hintMsg = _hint.Find("Msg").GetComponent<Text>();
            _hintCloseBtn = _hint.Find("CloseBtn").GetComponent<Button>();
            _hintMsg.alignment = TextAnchor.MiddleCenter;
            _update = transform.Find("UpdateNotice") as RectTransform;
            _updateTitle = _update.Find("Title").GetChild(0).GetComponent<Text>();
            _updateMsg = _update.Find("Msg").GetComponent<Text>();
            _updateCloseBtn = _update.Find("CloseBtn").GetComponent<Button>();
            _updateMsg.alignment = TextAnchor.UpperLeft;
            EventManager.Instance.AddListener(EventId.Connected, ConnectedCallback);
            EventManager.Instance.AddListener(EventId.Connecting, ConnectingCallback);
        }

        private void Start()
        {
            Close(ModalPanelType.Hint);
            Open("更新公告", "", ModalPanelType.Notice);
        }

        private void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener(EventId.Connected, ConnectedCallback);
            EventManager.Instance.RemoveListener(EventId.Connecting, ConnectingCallback);
        }

        private void ConnectingCallback()
        {
            switch (NetManager.Instance.CurApp)
            {
                case AppType.Client:
                    Open("网络消息", "正在连接登录服务器...", ModalPanelType.Hint);
                    break;
                case AppType.Login:
                    Open("网络消息", "正在连接游戏服务器...", ModalPanelType.Hint);
                    break;
                default:
                    break;
            }
        }

        private void ConnectedCallback() => Close();

        public void Open(string title, string msg, ModalPanelType type)
        {
            switch (type)
            {
                case ModalPanelType.Notice:
                    _updateTitle.text = title;
                    _updateMsg.text = msg;
                    _updateCloseBtn.onClick.AddListener(() => { Close(ModalPanelType.Notice); });
                    _update.gameObject.SetActive(true);
                    if (!HotUpdateManager.Instance.DownloadFile("AssetBundles/UpdateNotice.txt", $"{Application.persistentDataPath}/UpdateNotice.txt"))
                        EventManager.Instance.Invoke(EventId.HotUpdated, false);
                    else
                    {
                        _updateMsg.text = File.ReadAllText($"{Application.persistentDataPath}/UpdateNotice.txt");
                        HotUpdateManager.Instance.CheckUpdate();
                    }
                    break;
                case ModalPanelType.Hint:
                    _hintTitle.text = title;
                    _hintMsg.text = msg;
                    _hintCloseBtn.onClick.AddListener(() => { Close(ModalPanelType.Hint); });
                    _hint.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
            base.Open();
        }

        public override void Close()
        {
            _hint.gameObject.SetActive(false);
            _update.gameObject.SetActive(false);
            _hintCloseBtn.onClick.RemoveAllListeners();
            _updateCloseBtn.onClick.RemoveAllListeners();
            base.Close();
        }

        public void Close(ModalPanelType type)
        {
            switch (type)
            {
                case ModalPanelType.Notice:
                    _update.gameObject.SetActive(false);
                    _updateCloseBtn.onClick.RemoveAllListeners();
                    break;
                case ModalPanelType.Hint:
                    _hint.gameObject.SetActive(false);
                    _hintCloseBtn.onClick.RemoveAllListeners();
                    break;
                default:
                    break;
            }
        }

        public void SetMsg(string msg) { _hintMsg.text = msg; }
    }
}
