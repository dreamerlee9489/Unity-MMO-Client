using UnityEngine;

namespace Control.FSM
{
    public class Attack : AIState
    {
        public Attack(Entity owner, Entity target = null) : base(owner, target)
        {
            type = AIStateType.Attack;
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(Entity.Attack, true);
        }

        public override void Execute()
        {
            if (_target != null)
                _owner.transform.LookAt(_target.transform);
        }

        public override void Exit()
        {
            _owner.Anim.SetBool(Entity.Attack, false);
        }

        public override void UpdateState(int code)
        {
        }
    }
}
