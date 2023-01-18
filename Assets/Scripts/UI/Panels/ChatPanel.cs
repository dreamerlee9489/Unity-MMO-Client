using Manage;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum ChatType { Global, World, Team, Private }

    public class ChatPanel : BasePanel
	{
        private ToggleGroup chatToggles;
        private Toggle globalTog, worldTog, teamTog, privTog;
        private Transform chatRects;
        private ScrollRect globalRect, worldRect, teamRect, privRect, activeRect;
        private InputField inputField;
        private Button sendBtn;

        protected override void Awake()
        {
            base.Awake();
            chatToggles = transform.Find("ChatToggles").GetComponent<ToggleGroup>();
            globalTog = chatToggles.transform.Find("GlobalToggle").GetComponent<Toggle>();
            worldTog = chatToggles.transform.Find("WorldToggle").GetComponent<Toggle>();
            teamTog = chatToggles.transform.Find("TeamToggle").GetComponent<Toggle>();
            privTog = chatToggles.transform.Find("PrivateToggle").GetComponent<Toggle>();
            chatRects = transform.Find("ChatRects");
            globalRect = chatRects.Find("GlobalRect").GetComponent<ScrollRect>();
            worldRect = chatRects.Find("WorldRect").GetComponent<ScrollRect>();
            teamRect = chatRects.Find("TeamRect").GetComponent<ScrollRect>();
            privRect = chatRects.Find("PrivateRect").GetComponent<ScrollRect>();
            inputField = transform.Find("InputField").GetComponent<InputField>();

            chatToggles.allowSwitchOff = true;
            activeRect = globalRect;
            globalTog.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    activeRect.gameObject.SetActive(false);
                    globalRect.gameObject.SetActive(true);
                    activeRect = globalRect;
                }
                else if(!chatToggles.AnyTogglesOn())
                    globalTog.isOn = true;
            });
            worldTog.onValueChanged.AddListener((isOn) =>
            {
                if(isOn)
                {
                    activeRect.gameObject.SetActive(false);
                    worldRect.gameObject.SetActive(true);
                    activeRect = worldRect;
                }
                else if (!chatToggles.AnyTogglesOn())
                    worldTog.isOn = true;
            });
            teamTog.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) 
                {
                    activeRect.gameObject.SetActive(false);
                    teamRect.gameObject.SetActive(true);
                    activeRect = teamRect;
                }
                else if (!chatToggles.AnyTogglesOn())
                    teamTog.isOn = true;
            });
            privTog.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) 
                { 
                    activeRect.gameObject.SetActive(false);
                    privRect.gameObject.SetActive(true);
                    activeRect = privRect;
                }
                else if (!chatToggles.AnyTogglesOn())
                    privTog.isOn = true;
            });
            inputField.onSubmit.AddListener((str) =>
            {
                if (str.Length != 0)
                {
                    Proto.ChatMsg proto = new()
                    {
                        Sender = GameManager.Instance.mainPlayer.Sn,
                        Name = GameManager.Instance.mainPlayer.Name,
                        Content = inputField.text
                    };
                    inputField.text = "";
                    if (globalTog.isOn)
                        NetManager.Instance.SendPacket(Proto.MsgId.MiGlobalChat, proto);
                    else if (worldTog.isOn)
                        NetManager.Instance.SendPacket(Proto.MsgId.MiWorldChat, proto);
                    else if (teamTog.isOn)
                        NetManager.Instance.SendPacket(Proto.MsgId.MiTeamChat, proto);
                    else if (privTog.isOn)
                        NetManager.Instance.SendPacket(Proto.MsgId.MiPrivateChat, proto);
                }
            });
            PoolManager.Instance.Load("ChatMsg", "UI/ChatMsg", 40);
            Close();
        }

        public void ShowMsg(ChatType type, string msg)
        {
            Text text;
            switch (type)
            {
                case ChatType.Global:
                    text = PoolManager.Instance.Pop("ChatMsg", globalRect.content).GetComponent<Text>();
                    text.text = $"[全服] {msg}";
                    text.color = Color.yellow;
                    break;
                case ChatType.World:
                    text = PoolManager.Instance.Pop("ChatMsg", worldRect.content).GetComponent<Text>();
                    text.text = $"[地图] {msg}";
                    text.color = Color.green;
                    break;
                case ChatType.Team:
                    text = PoolManager.Instance.Pop("ChatMsg", teamRect.content).GetComponent<Text>();
                    text.text = $"[团队] {msg}";
                    text.color = Color.cyan;
                    break;
                case ChatType.Private:
                    text = PoolManager.Instance.Pop("ChatMsg", privRect.content).GetComponent<Text>();
                    text.text = $"[私聊] {msg}";
                    text.color = Color.white;
                    break;
                default:
                    break;
            }            
        }
    }
}
