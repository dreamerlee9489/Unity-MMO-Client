using Control;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    private RectTransform _hpBar, _xpBar;

    public Text nameTxt, teamTxt;
    public PlayerController player;

    protected override void Awake()
    {
        base.Awake();
        nameTxt = transform.Find("NameText").GetComponent<Text>();
        teamTxt = transform.Find("TeamText").GetComponent<Text>();
        _hpBar = transform.Find("HPBar").GetChild(1).GetComponent<RectTransform>();
        _xpBar = transform.Find("XPBar").GetChild(1).GetComponent<RectTransform>();
        Close();
    }

    public void InitPanel(Proto.AppearRole role, string teamTxt = "")
    {
        player = role.obj;
        nameTxt.text = role.name;
        this.teamTxt.text = teamTxt;
        _hpBar.localScale = new Vector3(player.hp * 1.0f / player.baseData.hp, 1, 1);
        if(player.xp * 1.0f / player.baseData.xp <= 1)
            _xpBar.localScale = new Vector3(player.xp * 1.0f / player.baseData.xp, 1, 1);
    }

    public void UpdateHp(int currHp)
    {
        if(currHp <= player.baseData.hp)
            _hpBar.localScale = new Vector3(currHp * 1.0f / player.baseData.hp, 1, 1);
    }

    public void UpdateXp(int currXp, int maxXp)
    {
        if(currXp <= player.baseData.xp)
            _xpBar.localScale = new Vector3(currXp * 1.0f / player.baseData.xp, 1, 1);
    }
}
