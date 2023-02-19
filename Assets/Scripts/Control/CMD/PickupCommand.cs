using Items;

namespace Control.CMD
{
    public interface IPicker : IExecutor
    {
        void Pickup(GameItem item);
        void UnPickup();
    }

    public class PickupCommand : ICommand
    {
        private GameItem _item;

        public PickupCommand(IPicker executor, GameItem item) : base(executor)
        {
            _item = item;
            (_executor as GameEntity).Target = item.transform;
            (_executor as IPicker).Pickup(_item);
        }

        public override CommandType GetCommandType() => CommandType.Pick;

        public override void Execute()
        {
            (_executor as IPicker).Pickup(_item);
        }

        public override void Undo()
        {
            base.Undo();
            (_executor as IPicker).UnPickup();
        }
    }
}
