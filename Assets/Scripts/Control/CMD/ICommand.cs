namespace Control.CMD
{
    public enum CommandType { None, Move, Attack, Pick, Teleport, Observe, Pvp, Death }
    public enum CommandState { Invaild, Running, Finish }

    public abstract class ICommand
    {
        protected IExecutor _executor;
        public CommandState state = CommandState.Invaild;

        public ICommand(IExecutor executor)
        {
            _executor = executor;
            state = CommandState.Running;
        }

        public abstract CommandType GetCommandType();

        public abstract void Execute();

        public virtual void Undo() { state = CommandState.Finish; }
    }

    public interface IExecutor
    {
    }
}
