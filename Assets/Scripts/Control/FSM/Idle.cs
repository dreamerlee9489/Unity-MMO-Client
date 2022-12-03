using Frame;
using UnityEngine;

namespace Control.FSM
{
    public class Idle : AIState
    {
        public Idle(EnemyController owner, PlayerController target = null) : base(owner, target)
        {
            type = AIStateType.Idle;
            Enter();
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.WalkSpeed;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {

        }

        public override void UpdateState(int code)
        {

        }
    }
}
