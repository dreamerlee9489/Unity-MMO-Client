using Manage;
using UnityEngine;

namespace Control.FSM
{
    public class Pursuit : FsmState
    {
        public Pursuit(FsmController owner, PlayerController target) : base(owner, target)
        {
            _type = FsmStateType.Pursuit;
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.RunSpeed;
        }

        public override void Execute()
        {
            _owner.Agent.destination = _target.transform.position;
            if (!_owner.CanSee(_target))
            {
                Net.FsmSyncState proto = new()
                {
                    EnemyId = _owner.Id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Idle,
                    Code = -1,
                };
                NetManager.Instance.SendPacket(Net.MsgId.C2SFsmSyncState, proto);
            }
            if (_owner.CanAttack(_target))
            {
                Net.FsmSyncState proto = new()
                {
                    EnemyId = _owner.Id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Attack,
                    Code = -1,
                };
                NetManager.Instance.SendPacket(Net.MsgId.C2SFsmSyncState, proto);
            }
        }

        public override void Exit()
        {
        }
    }
}
