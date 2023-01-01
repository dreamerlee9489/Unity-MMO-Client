using UnityEngine;

namespace Control.FSM
{
    public class Death : FsmState
    {
        public Death(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = FsmStateType.Death;
        }

        public override void Enter()
        {
            _owner.Agent.isStopped = true;
            _owner.Agent.radius = 0;
            _owner.Anim.SetBool(GameEntity.Death, true);
            _owner.GetComponent<CapsuleCollider>().enabled = false;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _owner.Agent.isStopped = false;
            _owner.Agent.radius = 0.3f;
            _owner.Anim.SetBool(GameEntity.Death, false);
            _owner.GetComponent<CapsuleCollider>().enabled = true;
        }
    }
}
