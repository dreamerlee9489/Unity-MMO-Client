namespace Control.BT
{
    public abstract class Action : Node
    {
        public Action(BtController npc) : base(npc)
        {
        }

        public abstract BtEventId GetEventId();
    }
}
