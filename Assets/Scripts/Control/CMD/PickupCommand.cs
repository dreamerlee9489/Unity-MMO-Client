using UnityEngine;

namespace Control.CMD
{
    public interface IPicker : IExecutor
    {
        void Pickup(Transform item);
        void UnPickup();
    }

    public class PickupCommand : ICommand
    {
        private Transform _item;

        public PickupCommand(IPicker executor, Transform item) : base(executor)
        {
            _item = item;
        }

        public override CommandType GetCommandType() => CommandType.Pickup;

        public override void Execute()
        {
            (_executor as IPicker).Pickup(_item);
        }

        public override void Undo()
        {
            (_executor as IPicker).UnPickup();
        }
    }
}
