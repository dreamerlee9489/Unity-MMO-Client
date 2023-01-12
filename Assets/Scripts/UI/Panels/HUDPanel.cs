using Control;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    private RectTransform _hpBar, _xpBar;

    public Text nameTxt, workTxt;
    public PlayerController player;

    protected override void Awake()
    {
        base.Awake();
        nameTxt = transform.Find("NameText").GetComponent<Text>();
        workTxt = transform.Find("WorkText").GetComponent<Text>();
        _hpBar = transform.Find("HPBar").GetChild(1).GetComponent<RectTransform>();
        _xpBar = transform.Find("XPBar").GetChild(1).GetComponent<RectTransform>();
        Close();
    }

    public void InitPanel(Proto.AppearRole role, string work = "")
    {
        player = role.obj;
        nameTxt.text = role.name;
        workTxt.text = work;
        _hpBar.localScale = new Vector3(player.hp * 1.0f / PlayerController.baseData.hp, 1, 1);
        if(player.xp * 1.0f / PlayerController.baseData.xp <= 1)
            _xpBar.localScale = new Vector3(player.xp * 1.0f / PlayerController.baseData.xp, 1, 1);
    }

    public void UpdateHp(int currHp)
    {
        if(currHp <= PlayerController.baseData.hp)
            _hpBar.localScale = new Vector3(currHp * 1.0f / PlayerController.baseData.hp, 1, 1);
    }

    public void UpdateXp(int currXp, int maxXp)
    {
        if(currXp <= PlayerController.baseData.xp)
            _xpBar.localScale = new Vector3(currXp * 1.0f / PlayerController.baseData.xp, 1, 1);
    }
}
