using Manage;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChatPanel : BasePanel
	{
        private RectTransform content;

        protected override void Awake()
        {
            base.Awake();
            content = GetComponent<ScrollRect>().content;
            PoolManager.Instance.Load("ChatMsg", "UI/ChatMsg");
            Close();
        }

        public void ShowMsg(string msg, Color color)
        {
            Text text = PoolManager.Instance.Pop("ChatMsg", content).GetComponent<Text>();
            text.text = msg;
            text.color = color;
        }
    }
}
