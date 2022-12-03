using Frame;
using Net;
using System.Collections;
using UnityEngine;

namespace Control.FSM
{
    public class Pursuit : AIState
    {
        private WaitForSeconds _waitForSeconds = new(0.5f);

        public Pursuit(EnemyController owner, PlayerController target) : base(owner, target)
        {
            type = AIStateType.Pursuit;
            Enter();
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.RunSpeed;
        }

        public override void Execute()
        {
            _owner.Agent.destination = _target.transform.position;
        }

        public override void Exit()
        {
        }

        public override void UpdateState(int code)
        {
        }
    }
}
