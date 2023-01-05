using UnityEngine;

namespace Control.CMD
{
    public class PickupCommand : ICommand
    {
        private Transform _item;

        public PickupCommand(IPickupExecutor executor, Transform item) : base(executor)
        {
            _item = item;
        }

        public override CommandType GetCommandType() => CommandType.Pickup;

        public override void Execute()
        {
            (_executor as IPickupExecutor).Pickup(_item);
        }

        public override void Undo()
        {
            (_executor as IPickupExecutor).UnPickup();
        }
    }
}
