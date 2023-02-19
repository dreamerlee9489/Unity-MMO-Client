namespace Control.CMD
{
    public interface IObserver : IExecutor
    {
        void Observe(PlayerController target);
        void UnObserve();
    }

    public class ObserveCommand : ICommand
    {
        private readonly PlayerController _target;

        public ObserveCommand(IExecutor executor, PlayerController target) : base(executor)
        {
            _target = target;
            (_executor as GameEntity).Target = target.transform;
            (_executor as IObserver).Observe(_target);
        }

        public override void Execute()
        {
            (_executor as IObserver).Observe(_target);
        }

        public override void Undo()
        {
            base.Undo();
            (_executor as IObserver).UnObserve();
        }

        public override CommandType GetCommandType() => CommandType.Observe;
    }
}
