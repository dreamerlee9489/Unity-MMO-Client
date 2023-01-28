namespace Control.BT
{
    public class ActionFlee : Action
    {
        public ActionFlee(BtController npc) : base(npc)
        {
        }

        protected override void Enter()
        {
            npc.Agent.speed = npc.runSpeed;
            npc.Agent.destination = npc.initPos;
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
