namespace Control.BT
{
    public class ActionPursue : Action
    {
        public ActionPursue(BtController npc) : base(npc)
        {
        }

        protected override void Enter()
        {
            npc.Agent.speed = npc.runSpeed;
        }

        protected override BtStatus Execute()
        {
            npc.Agent.destination = npc.Target.position;
            return BtStatus.Running;
        }

        protected override void Exit()
        {
        }
    }
}
