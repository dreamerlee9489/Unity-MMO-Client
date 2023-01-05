namespace Control.CMD
{
    public enum CommandType { None, Move, Attack, Pickup, Teleport }

    public abstract class ICommand
    {
        protected IExecutor _executor;

        public ICommand(IExecutor executor)
        {
            _executor = executor;
        }

        public abstract CommandType GetCommandType();

        public abstract void Execute();

        public abstract void Undo();
    }

    public interface IExecutor
    {
    }
}
