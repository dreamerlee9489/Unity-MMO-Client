using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class PopupPanel : BasePanel
	{
		private Text _text;
		private Button _yesBtn, _noBtn;

        protected override void Awake()
        {
            base.Awake();
            _text = transform.Find("Text").GetComponent<Text>();
            _yesBtn = transform.Find("YesBtn").GetComponent<Button>();
            _noBtn = transform.Find("NoBtn").GetComponent<Button>();
            Close();
        }

        public void Open(string msg, UnityAction yesFunc, UnityAction noFunc)
        {
            _text.text = msg;
            if(yesFunc != null && noFunc != null)
            {
                _yesBtn.onClick.AddListener(yesFunc);
                _noBtn.onClick.AddListener(noFunc);
            }
            _yesBtn.onClick.AddListener(Close);
            _noBtn.onClick.AddListener(Close);
            base.Open();
        }

        public override void Close()
        {
            _yesBtn.onClick.RemoveAllListeners();
            _noBtn.onClick.RemoveAllListeners();
            base.Close();
        }
    }
}
