using Control;
using Manage;
using UI;
using UnityEngine.UI;

public class PropPanel : BasePanel
{
    private Text idTxt, lvTxt, xpTxt, hpTxt, mpTxt, atkTxt, defTxt;
    private PlayerController player;
    private PlayerBaseData baseData;

    protected override void Awake()
    {
        base.Awake();
        idTxt = transform.Find("IDText").GetChild(0).GetComponent<Text>();
        lvTxt = transform.Find("LVText").GetChild(0).GetComponent<Text>();
        xpTxt = transform.Find("XPText").GetChild(0).GetComponent<Text>();
        hpTxt = transform.Find("HPText").GetChild(0).GetComponent<Text>();
        mpTxt = transform.Find("MPText").GetChild(0).GetComponent<Text>();
        atkTxt = transform.Find("AtkText").GetChild(0).GetComponent<Text>();
        defTxt = transform.Find("DefText").GetChild(0).GetComponent<Text>();
        Close();
    }

    public void InitPanel()
    {
        player = GameManager.Instance.mainPlayer.Obj.GetComponent<PlayerController>();
        baseData = GameManager.Instance.playerBaseDatas[player.lv];
        idTxt.text = GameManager.Instance.mainPlayer.Name;
        lvTxt.text = player.lv.ToString();
        xpTxt.text = $"{player.xp} / {baseData.xp}";
        hpTxt.text = $"{player.hp} / {baseData.hp}";
        mpTxt.text = $"{player.mp} / {baseData.mp}";
        atkTxt.text = player.atk.ToString();
        defTxt.text = player.def.ToString();
    }

    public void UpdateHp(int currHp)
    {
        if(currHp <= baseData.hp) 
            hpTxt.text = $"{currHp} / {baseData.hp}";
    }

    public void UpdateXp(int currXp)
    {
        if(currXp <= baseData.xp)
            xpTxt.text = $"{currXp} / {baseData.xp}";
    }
}
