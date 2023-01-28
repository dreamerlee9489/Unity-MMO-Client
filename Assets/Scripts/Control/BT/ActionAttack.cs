namespace Control.BT
{
    public class ActionAttack : Action
    {
        public ActionAttack(BtController npc) : base(npc)
        {
        }

        protected override void Enter()
        {
            npc.Agent.speed = npc.walkSpeed;
            npc.Agent.destination = npc.Target.position;
            npc.Anim.SetBool(GameEntity.attack, true);
        }

        protected override BtStatus Execute()
        {
            npc.transform.LookAt(npc.Target);
            return BtStatus.Running;
        }

        protected override void Exit()
        {           
            npc.Anim.SetBool(GameEntity.attack, false);
        }
    }
}
