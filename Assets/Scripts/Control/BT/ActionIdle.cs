using UnityEngine;

namespace Control.BT
{
    public class ActionIdle : Action
    {
        public ActionIdle(BtController npc) : base(npc)
        {
        }

        public override BtEventId GetEventId() => BtEventId.Idle;

        protected override void Enter()
        {
            npc.Agent.speed = npc.walkSpeed;
            npc.Agent.isStopped = true;
        }

        protected override BtStatus Execute()
        {
            return BtStatus.Running;
        }

        protected override void Exit()
        {
            npc.Agent.isStopped = false;
        }
    }
}
