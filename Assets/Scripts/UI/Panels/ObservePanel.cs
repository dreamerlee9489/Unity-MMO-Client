using Control;
using Manage;
using UnityEngine.UI;

namespace UI
{
    public class ObservePanel : BasePanel
    {
        private PlayerController _player;
        private Button _propBtn, _teamBtn, _fightBtn, _dealBtn;

        protected override void Awake()
        {
            base.Awake();
            _propBtn = transform.Find("PropBtn").GetComponent<Button>();
            _teamBtn = transform.Find("TeamBtn").GetComponent<Button>();
            _fightBtn = transform.Find("FightBtn").GetComponent<Button>();
            _dealBtn = transform.Find("DealBtn").GetComponent<Button>();

            _propBtn.onClick.AddListener(() =>
            {
                Close();
            });
            _teamBtn.onClick.AddListener(() => 
            { 
                Proto.ReqJoinTeam proto = new() 
                { 
                    Applicant = GameManager.Instance.mainPlayer.Sn,
                    Responder = _player.Sn 
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CReqJoinTeam, proto);
                Close();
            });
            _fightBtn.onClick.AddListener(() => 
            {
                Close();
            });
            _dealBtn.onClick.AddListener(() => 
            { 
                Close(); 
            });
            Close();
        }

        public void SetPlayer(PlayerController player) => _player = player;
    }
}
