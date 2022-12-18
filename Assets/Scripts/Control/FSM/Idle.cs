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
            _owner.Agent.isStopped = true;
            _target = GameManager.Instance.MainPlayer.Obj.GetComponent<PlayerController>();
        }

        public override void Execute()
        {
            if (_owner.CanSee(_target))
            {
                Net.FsmSyncState proto = new()
                {
                    EnemyId = _owner.id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Pursuit,
                    Code = -1,
                };
                NetManager.Instance.SendPacket(Net.MsgId.C2SFsmSyncState, proto);
            }
        }

        public override void Exit()
        {
            _owner.Agent.isStopped = false;
        }
    }
}
