namespace Control.BT
{
    public class ActionFlee : Action
    {
        public ActionFlee(BtController npc) : base(npc)
        {
        }

        public override BtEventId GetEventId() => BtEventId.Flee;

        protected override void Enter()
        {
            npc.Agent.speed = npc.runSpeed;
            //npc.Agent.destination = npc.initPos;
            npc.ReqMoveTo(npc.initPos, true);
        }

        protected override BtStatus Execute()
        {
            return BtStatus.Running;
        }

        protected override void Exit()
        {
            npc.Agent.speed = npc.walkSpeed;
        }
    }
}
