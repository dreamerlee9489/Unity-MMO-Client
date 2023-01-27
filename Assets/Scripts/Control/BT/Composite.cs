using System.Collections.Generic;

namespace Control.BT
{
    public abstract class Composite : Node
    {
        protected Node curr = null;
        protected List<Node> children = new();

        public Composite(BtController npc) : base(npc)
        {
        }

        public virtual void AddChild(Node child) => children.Add(child);

        public virtual void RemoveChild(Node child) => children.Remove(child);
    }
}
