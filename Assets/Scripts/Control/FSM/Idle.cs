using UnityEngine;

namespace Control.FSM
{
    public class Idle : AIState
    {
        public Idle(Entity owner, Entity target = null) : base(owner, target)
        {
            type = AIStateType.Idle;
            Enter();
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(Entity.Attack, false);
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
