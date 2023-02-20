namespace Control.CMD
{
    public interface IExecutor { }

    public enum CommandType { None, Move, Attack, Pick, Teleport, Observe, Pvp, Death }
    public enum CommandState { Invaild, Running, Finish }

    public abstract class ICommand
    {
        protected IExecutor _executor;

        public ICommand(IExecutor executor)
        {
            _executor = executor;
        }

        public abstract CommandType GetCmdType();

        public abstract void Execute();

        public abstract void Undo();
    }
}
