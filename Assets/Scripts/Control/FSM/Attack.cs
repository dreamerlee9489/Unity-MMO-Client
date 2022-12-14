﻿using Manage;

namespace Control.FSM
{
    public class Attack : FsmState
    {
        public Attack(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = FsmStateType.Attack;
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, true);
            _owner.Agent.speed = _owner.WalkSpeed;
            _owner.Agent.destination = _target.transform.position;
        }

        public override void Execute()
        {
            _owner.transform.LookAt(_target.transform);
            if (!_owner.CanSee(_target))
            {
                Proto.FsmSyncState proto = new()
                {
                    EnemyId = _owner.Id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Idle,
                    Code = -1,
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SFsmSyncState, proto);
            }
            if (!_owner.CanAttack(_target))
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
            _owner.Anim.SetBool(GameEntity.Attack, false);
        }
    }
}
