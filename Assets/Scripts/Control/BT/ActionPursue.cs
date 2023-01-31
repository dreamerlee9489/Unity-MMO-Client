namespace Control.BT
{
    public class ActionPursue : Action
    {
        public ActionPursue(BtController npc) : base(npc)
        {
        }

        public override BtEventId GetEventId() => BtEventId.Pursue;

        protected override void Enter()
        {
            npc.Agent.speed = npc.runSpeed;
        }

        protected override BtStatus Execute()
        {
            npc.ReqMoveTo(npc.Target.position, true);
            return BtStatus.Running;
        }

        protected override void Exit()
        {
        }
    }
}
