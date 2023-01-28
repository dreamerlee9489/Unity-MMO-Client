using UnityEngine;
using UnityEngine.AI;

namespace Control.BT
{
    public class ActionBirth : Action
    {
        public ActionBirth(BtController npc) : base(npc)
        {
        }

        protected override void Enter()
        {
        }

        protected override BtStatus Execute()
        {
            return BtStatus.Running;
        }

        protected override void Exit()
        {
            npc.hp = npc.initHp;
            npc.NameBar.HpBar.UpdateHp(npc.hp, npc.initHp, false);            
            npc.Agent.radius = 0.3f;
            npc.Agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            npc.Anim.SetBool(GameEntity.death, false);
            npc.GetComponent<CapsuleCollider>().enabled = true;
            npc.gameObject.SetActive(true);
        }
    }
}
