using Net;
using UnityEngine;

namespace Control.FSM
{
    public class Attack : AIState
    {
        public Attack(EnemyController owner, PlayerController target = null) : base(owner, target)
        {
            type = AIStateType.Attack;
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
        }

        public override void Exit()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
        }

        public override void UpdateState(int code)
        {
        }
    }
}
