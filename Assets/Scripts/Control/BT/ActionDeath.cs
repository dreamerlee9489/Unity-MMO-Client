using Manage;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Control.BT
{
    public class ActionDeath : Action
    {
        public ActionDeath(BtController npc) : base(npc)
        {
        }

        public override BtEventId GetEventId() => BtEventId.Death;

        protected override void Enter()
        {
            npc.Agent.isStopped = true;
            npc.Agent.radius = 0;
            npc.Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            npc.Anim.SetBool(GameEntity.death, true);
            npc.GetComponent<CapsuleCollider>().enabled = false;
            npc.LinkPlayer(false);
            MonoManager.Instance.StartCoroutine(CleanUp());
        }

        protected override BtStatus Execute()
        {
            return BtStatus.Running;
        }

        protected override void Exit()
        {
            MonoManager.Instance.StopCoroutine(CleanUp());
        }

        private IEnumerator CleanUp()
        {
            yield return new WaitForSeconds(3);
            npc.Agent.isStopped = false;
            npc.gameObject.SetActive(false);          
            npc.root.SwitchNode(BtEventId.Birth);
        }
    }
}
