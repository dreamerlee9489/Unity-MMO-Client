namespace Control.BT
{
    public class Repeat : Decorator
    {
        private int count = 0, limit = int.MaxValue;

        public Repeat(BtController npc, Node child, int limit = int.MaxValue) : base(npc, child)
        {
            this.limit = limit;
        }

        protected override void Enter()
        {
            throw new System.NotImplementedException();
        }

        protected override BtStatus Execute()
        {
            throw new System.NotImplementedException();
        }

        protected override void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}
