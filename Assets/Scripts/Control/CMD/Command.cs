using UnityEngine;

namespace Control.CMD
{
    public abstract class Command
    {
        protected readonly ICmdReceiver _receiver = null;

        protected Command(ICmdReceiver receiver) => _receiver = receiver;

        public abstract void Execute(Vector3 point);

        public abstract void Execute(Transform transform);

        public abstract void Undo();
    }
}
