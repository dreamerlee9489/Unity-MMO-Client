using UI;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    RectTransform hpBar, xpBar;
    Text goldTxt;

    protected override void Awake()
    {
        base.Awake();
        hpBar = transform.Find("HPBar").GetChild(1).GetComponent<RectTransform>();
        xpBar = transform.Find("XPBar").GetChild(1).GetComponent<RectTransform>();
        goldTxt = transform.Find("GoldBar").GetChild(1).GetComponent<Text>();
        Close();
    }

    public void UpdateHp(int currHp, int maxHp)
    {
        hpBar.localScale = new Vector3(currHp * 1.0f / maxHp, 1, 1);
    }

    public void UpdateXp(int currXp, int maxXp)
    {
        xpBar.localScale = new Vector3(currXp * 1.0f / maxXp, 1, 1);
    }

    public void UpdateGold(int currGold)
    {
        goldTxt.text = currGold.ToString();
    }
}
