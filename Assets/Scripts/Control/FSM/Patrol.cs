using UnityEngine;

namespace Control.FSM
{
    public class Patrol : AIState
    {
        private PatrolPath _patrolPath;

        public Patrol(Entity owner, Entity target = null) : base(owner, target)
        {
            type = AIStateType.Patrol;
            Enter();
        }

        public override void Enter()
        {
            _owner.Agent.speed = Entity.WalkSpeed;
            _patrolPath = _owner.GetComponent<EnemyController>().PatrolPath;
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
