using UnityEngine;
using UnityEngine.UI;

namespace UI.Panel
{
    public class ModalBox : MonoBehaviour
    {
        private Button _closeBtn;
        private ModalPanel _modalPanel;
        public Text Title { get; set; }
        public Text Msg { get; set; }

        void Awake()
        {
            _closeBtn = transform.Find("CloseBtn").GetComponent<Button>();
            Title = transform.Find("Title").GetChild(0).GetComponent<Text>();
            Msg = transform.Find("Msg").GetComponent<Text>();
            _modalPanel = transform.parent.GetComponent<ModalPanel>();
            _closeBtn.onClick.AddListener(() =>
            {
                _modalPanel.Close();
            });
        }
    }
}
