using UnityEngine;

namespace Control.FSM
{
    public class Idle : AIState
    {
        public Idle(Entity owner, Entity target = null) : base(owner, target) => Enter();

        public override void Enter()
        {
            throw new System.NotImplementedException();
        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }

        public override void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}
