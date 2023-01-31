namespace Control.BT
{
    public class ActionPatrol : Action
    {
        public ActionPatrol(BtController npc) : base(npc)
        {
        }

        public override BtEventId GetEventId() => BtEventId.Patrol;

        protected override void Enter()
        {
            npc.Agent.isStopped = false;
            npc.Agent.speed = npc.walkSpeed;
        }

        protected override BtStatus Execute()
        {
            npc.ReqMoveTo(npc.patrolPath.CurrPoint, false);
            return BtStatus.Running;
        }

        protected override void Exit()
        {
        }
    }
}
