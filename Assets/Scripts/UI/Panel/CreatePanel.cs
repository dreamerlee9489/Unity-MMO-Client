using Manage;
using UnityEngine.UI;

namespace UI
{
    public class CreatePanel : BasePanel
    {
        private ToggleGroup _toggleGroup;
        private Toggle _maleToggle;
        private InputField _roleName;
        private Button _createBtn;

        protected override void Awake()
        {
            base.Awake();
            _toggleGroup = transform.Find("ToggleGroup").GetComponent<ToggleGroup>();
            _maleToggle = _toggleGroup.transform.Find("MaleToggle").GetComponent<Toggle>();
            _roleName = transform.Find("RoleName").GetComponent<InputField>();
            _createBtn = transform.Find("CreateBtn").GetComponent<Button>();

            _toggleGroup.allowSwitchOff = false;
            _createBtn.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(_roleName.text))
                {
                    GameManager.Instance.Canvas.GetPanel<ModalPanel>().Open("创建角色", "角色名不能为空", ModalPanelType.Hint);
                    return;
                }
                Proto.CreatePlayer proto = new()
                {
                    Name = _roleName.text,
                    Gender = _maleToggle.isOn ? Proto.Gender.Male : Proto.Gender.Female
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2LCreatePlayer, proto);
                Close();
            });
        }

        protected override void Start()
        {
            base.Start();
            Close();
        }
    }
}
