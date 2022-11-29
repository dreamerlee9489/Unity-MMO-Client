using UnityEngine;

namespace Control.FSM
{
    public class Patrol : AIState
    {
        public Patrol(Entity owner, Entity target = null) : base(owner, target) => Enter();

        public override void Enter()
        {
            _owner.Agent.speed = Entity.WalkSpeed;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            Debug.Log("Patrol Exit: " + _owner.name);
        }
    }
}
