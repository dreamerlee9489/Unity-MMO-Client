using Frame;
using Net;
using UnityEngine;

namespace Control.FSM
{
    public class Patrol : AIState
    {
        private PatrolPath _patrolPath;

        public Patrol(EnemyController owner, PlayerController target = null) : base(owner, target)
        {
            type = AIStateType.Patrol;
            Enter();
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.WalkSpeed;
            _patrolPath = _owner.PatrolPath;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
        }

        public override void UpdateState(int code)
        {
            _owner.Agent.destination = _patrolPath.Path[code].position;
        }
    }
}
