using System.Collections.Generic;
using UI;

namespace Manage
{
    public class TeamManager : BaseSingleton<TeamManager>
	{
        private ulong _mainSn;
        public Dictionary<ulong, HUDPanel> teamDict = new();

        public void Initial()
		{
            _mainSn = GameManager.Instance.mainPlayer.Sn;
            PoolManager.Instance.LoadPush(PoolType.HUDPanel, "UI/Panel/HUDPanel", 4);
            HUDPanel panel = PoolManager.Instance.Pop(PoolType.HUDPanel, UIManager.Instance.hudGroup).GetComponent<HUDPanel>();
            panel.InitPanel(GameManager.currWorld.roleDict[_mainSn]);
            teamDict.Add(_mainSn, panel);
        }

        public void Destroy()
        {
            teamDict.Clear();
            int count = UIManager.Instance.hudGroup.childCount;
            for (int i = 0; i < count; i++)
                PoolManager.Instance.Push(PoolType.HUDPanel, UIManager.Instance.hudGroup.GetChild(0).gameObject);
        }

        public void ParseReqJoinTeam(Proto.ReqJoinTeam proto)
        {
            string text = $"玩家[{GameManager.currWorld.roleDict[proto.Applicant].name}]申请入队，是否同意？";
            UIManager.Instance.GetPanel<PopupPanel>().Open(text, () =>
            {
                teamDict[_mainSn].workTxt.text = "队长";
                HUDPanel panel = PoolManager.Instance.Pop(PoolType.HUDPanel, UIManager.Instance.hudGroup).GetComponent<HUDPanel>();
                panel.InitPanel(GameManager.currWorld.roleDict[proto.Applicant], "队员");
                teamDict.Add(proto.Applicant, panel);
                Proto.JoinTeamRes joinRes = new()
                {
                    Applicant = proto.Applicant,
                    Responder = _mainSn,
                    Agree = true
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CJoinTeamRes, joinRes);
            }, () =>
            {
                Proto.JoinTeamRes joinRes = new()
                {
                    Applicant = proto.Applicant,
                    Responder = _mainSn,
                    Agree = false
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CJoinTeamRes, joinRes);
            });
        }

        public void ParseJoinTeamRes(Proto.JoinTeamRes proto)
        {
            if (!proto.Agree)
            {
                string text = $"玩家[{GameManager.currWorld.roleDict[proto.Responder].name}]拒绝了你的入队请求。";
                UIManager.Instance.GetPanel<PopupPanel>().Open(text, null, null);
            }
            else
            {
                teamDict.Clear();
                int count = UIManager.Instance.hudGroup.childCount;
                for (int i = 0; i < count; i++)
                    PoolManager.Instance.Push(PoolType.HUDPanel, UIManager.Instance.hudGroup.GetChild(0).gameObject);
                for (int i = 0; i < proto.Members.Count; i++)
                {
                    HUDPanel panel = PoolManager.Instance.Pop(PoolType.HUDPanel, UIManager.Instance.hudGroup).GetComponent<HUDPanel>();
                    panel.InitPanel(GameManager.currWorld.roleDict[proto.Members[i].MemberSn], i == 0 ? "队长" : "队员");
                    teamDict.Add(proto.Members[i].MemberSn, panel);
                }
            }
        }
    }
}
