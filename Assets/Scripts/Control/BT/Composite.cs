using System.Collections.Generic;

namespace Control.BT
{
    public abstract class Composite : Node
    {
        protected List<Action> children = new();

        public Action curNode = null;

        public Composite(BtController npc) : base(npc)
        {
        }

        public virtual void AddChild(Action child) => children.Add(child);

        public virtual void RemoveChild(Action child) => children.Remove(child);
    }
}
