using UnityEngine;

namespace Control.FSM
{
    public class Pursuit : AIState
    {
        WaitForSeconds _wait1s = new WaitForSeconds(1);

        public Pursuit(Entity owner, Entity target) : base(owner, target)
        {
            type = AIStateType.Pursuit;
            Enter();
        }

        public override void Enter()
        {
            _owner.Agent.speed = Entity.RunSpeed;
        }

        public override void Execute()
        {
            if (_target)
            {
                _owner.Agent.destination = _target.transform.position;
            }
        }

        public override void Exit()
        {
            _owner.Agent.speed = Entity.WalkSpeed;
        }

        public override void UpdateState(int code)
        {
        }
    }
}
