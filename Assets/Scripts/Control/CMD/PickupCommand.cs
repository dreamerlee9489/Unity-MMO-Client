using Items;
using UnityEngine;

namespace Control.CMD
{
    public class PickupCommand : ICommand
    {
        private Vector3 _point;

        public PickupCommand(IPickupExecutor executor, Vector3 point) : base(executor)
        {
            _point = point;
        }

        public override CommandType GetCommandType() => CommandType.Pickup;

        public override void Execute()
        {
            (_executor as IPickupExecutor).Pickup(_point);
        }

        public override void Undo()
        {
            (_executor as IPickupExecutor).UnPickup();
        }
    }
}
