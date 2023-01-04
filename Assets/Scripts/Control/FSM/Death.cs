using UnityEngine;
using UnityEngine.AI;

namespace Control.FSM
{
    public class Death : State
    {
        public Death(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = StateType.Death;
        }

        public override void Enter()
        {
            _owner.Agent.radius = 0;
            _owner.Agent.isStopped = true;
            _owner.Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            _owner.Anim.SetBool(GameEntity.death, true);
            _owner.GetComponent<CapsuleCollider>().enabled = false;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _owner.Agent.radius = 0.3f;
            _owner.Agent.isStopped = false;
            _owner.Agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            _owner.Anim.SetBool(GameEntity.death, false);
            _owner.GetComponent<CapsuleCollider>().enabled = true;
        }
    }
}
