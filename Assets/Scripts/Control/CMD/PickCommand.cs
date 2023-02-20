using Items;

namespace Control.CMD
{
    public interface IPicker : IExecutor
    {
        void Pick(GameItem item);
        void UnPick();
    }

    public class PickCommand : ICommand
    {
        private GameItem _item;

        public PickCommand(IPicker executor, GameItem item) : base(executor)
        {
            _item = item;
            (_executor as GameEntity).Target = item.transform;
            (_executor as IPicker).Pick(_item);
        }

        public override CommandType GetCmdType() => CommandType.Pick;

        public override void Execute()
        {
            (_executor as IPicker).Pick(_item);
        }

        public override void Undo()
        {
            (_executor as IPicker).UnPick();
        }
    }
}
