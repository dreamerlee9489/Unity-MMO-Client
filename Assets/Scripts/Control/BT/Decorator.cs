namespace Control.BT
{
    public abstract class Decorator : Node
    {
        protected Node child = null;

        public Decorator(BtController npc, Node child) : base(npc)
        {
            this.child = child;
        }
    }
}
