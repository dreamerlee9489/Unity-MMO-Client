using UnityEngine;

namespace Control.CMD
{
    public class CombatCommand : Command
    {
        public CombatCommand(ICmdReceiver receiver) : base(receiver) { }

        public override void Execute(Vector3 point) => _receiver.Execute(point);

        public override void Execute(Transform transform) => _receiver.Execute(transform);

        public override void Undo() => _receiver.Undo();
    }
}
