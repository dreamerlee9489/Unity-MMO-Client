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
            npc.Anim.SetBool(GameEntity.attack, true);
        }

        protected override BtStatus Execute()
        {
            if (npc.Target != null)
            {
                npc.transform.LookAt(npc.Target);
                npc.Agent.destination = npc.Target.position;
            }
            return BtStatus.Running;
        }

        protected override void Exit()
        {           
            npc.Anim.SetBool(GameEntity.attack, false);
        }
    }
}
