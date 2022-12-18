using Manage;
using UnityEngine;

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
            _owner.Agent.speed = _owner.WalkSpeed;
            _owner.Agent.destination = _target.transform.position;
            _owner.Anim.SetBool(GameEntity.Attack, true);
        }

        public override void Execute()
        {
            _owner.transform.LookAt(_target.transform);
            if (!_owner.CanSee(_target))
            {
                Net.FsmSyncState proto = new()
                {
                    EnemyId = _owner.id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Idle,
                    Code = -1,
                };
                NetManager.Instance.SendPacket(Net.MsgId.C2SFsmSyncState, proto);
            }
            if (!_owner.CanAttack(_target))
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
            _owner.Anim.SetBool(GameEntity.Attack, false);
        }
    }
}
