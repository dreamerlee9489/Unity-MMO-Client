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
            _owner.agent.radius = 0;
            _owner.agent.isStopped = true;
            _owner.agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            _owner.anim.SetBool(GameEntity.death, true);
            _owner.GetComponent<CapsuleCollider>().enabled = false;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _owner.currState = null;
            _owner.agent.radius = 0.3f;
            _owner.agent.isStopped = false;
            _owner.agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            _owner.anim.SetBool(GameEntity.death, false);
            _owner.GetComponent<CapsuleCollider>().enabled = true;
        }
    }
}
