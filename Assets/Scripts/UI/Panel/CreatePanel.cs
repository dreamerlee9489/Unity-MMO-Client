﻿using Frame;
using Net;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreatePanel : BasePanel
    {
        ToggleGroup _toggleGroup = null;
        Toggle _femaleToggle = null, _maleToggle = null;
        InputField _roleName = null;
        Button _createBtn = null;

        protected override void Awake()
        {
            base.Awake();
            _panelType = PanelType.CreatePanel;
            _toggleGroup = transform.Find("ToggleGroup").GetComponent<ToggleGroup>();
            _maleToggle = _toggleGroup.transform.Find("MaleToggle").GetComponent<Toggle>();
            _femaleToggle = _toggleGroup.transform.Find("FemaleToggle").GetComponent<Toggle>();
            _roleName = transform.Find("RoleName").GetComponent<InputField>();
            _createBtn = transform.Find("CreateBtn").GetComponent<Button>();

            _toggleGroup.allowSwitchOff = false;
            _createBtn.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(_roleName.text))
                {
                    GameManager.Instance.Canvas.GetPanel<ModalPanel>().Open("创建角色", "角色名不能为空");
                    return;
                }
                Proto.CreatePlayer proto = new Proto.CreatePlayer();
                proto.Name = _roleName.text;
                proto.Gender = _maleToggle.isOn ? Proto.Gender.Male : Proto.Gender.Female;
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
