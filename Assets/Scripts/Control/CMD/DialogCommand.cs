using UnityEngine;

namespace Control.CMD
{
    public interface IObserver : IExecutor
    {
        void Observe(Transform target);
        void UnObserve();
    }

    public class ObserveCommand : ICommand
    {
        private readonly Transform _target;

        public ObserveCommand(IExecutor executor, Transform target) : base(executor)
        {
            _target = target;
        }

        public override void Execute()
        {
            (_executor as IObserver).Observe(_target);
        }

        public override void Undo()
        {
            (_executor as IObserver).UnObserve();
        }

        public override CommandType GetCommandType() => CommandType.Dialog;
    }
}
