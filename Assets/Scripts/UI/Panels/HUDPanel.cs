using Manage;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    private Text _nameBar, _teamBar;
    private RectTransform _hpBar, _xpBar;

    protected override void Awake()
    {
        base.Awake();
        _nameBar = transform.Find("NameBar").GetComponent<Text>();
        _teamBar = transform.Find("TeamBar").GetComponent<Text>();
        _hpBar = transform.Find("HPBar").GetChild(1).GetComponent<RectTransform>();
        _xpBar = transform.Find("XPBar").GetChild(1).GetComponent<RectTransform>();
        Close();
    }

    public override void Open()
    {
        base.Open();
        _nameBar.text = GameManager.Instance.MainPlayer.Name;
    }

    public void UpdateHp(int currHp, int maxHp)
    {
        if(currHp <= maxHp)
            _hpBar.localScale = new Vector3(currHp * 1.0f / maxHp, 1, 1);
    }

    public void UpdateXp(int currXp, int maxXp)
    {
        if(currXp <= maxXp)
            _xpBar.localScale = new Vector3(currXp * 1.0f / maxXp, 1, 1);
    }
}
