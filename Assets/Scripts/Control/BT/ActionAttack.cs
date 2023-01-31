namespace Control.BT
{
    public class ActionAttack : Action
    {
        public ActionAttack(BtController npc) : base(npc)
        {
        }

        public override BtEventId GetEventId() => BtEventId.Attack;

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
                npc.ReqMoveTo(npc.Target.position, false);
            }
            return BtStatus.Running;
        }

        protected override void Exit()
        {           
            npc.Anim.SetBool(GameEntity.attack, false);
        }
    }
}
