using Manage;
using UnityEngine;

namespace Control.FSM
{
    public class Idle : FsmState
    {
        public Idle(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = FsmStateType.Idle;
        }

        public override void Enter()
        {
            _owner.Agent.speed = _owner.WalkSpeed;
            if (_owner.gameObject.activeSelf)
                _owner.Agent.isStopped = true;
            _target = GameManager.Instance.MainPlayer.GetGameObject().GetComponent<PlayerController>();
        }

        public override void Execute()
        {
            if (_owner.CanSee(_target))
            {
                Proto.FsmSyncState proto = new()
                {
                    EnemyId = _owner.Id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Pursuit,
                    Code = -1,
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SFsmSyncState, proto);
            }
        }

        public override void Exit()
        {
            _owner.Agent.isStopped = false;
        }
    }
}
