using UnityEngine;

namespace Control.BT
{
    public class ActionPatrol : Action
    {
        public ActionPatrol(BtController npc) : base(npc)
        {
        }

        protected override void Enter()
        {
            npc.Agent.isStopped = false;
            npc.Agent.speed = npc.walkSpeed;
            npc.Agent.destination = npc.patrolPath.GetCurrPoint();
        }

        protected override BtStatus Execute()
        {
            return BtStatus.Running;
        }

        protected override void Exit()
        {
        }
    }
}
