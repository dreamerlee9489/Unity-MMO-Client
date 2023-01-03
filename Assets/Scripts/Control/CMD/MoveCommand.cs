using UnityEngine;

namespace Control.CMD
{
    public class MoveCommand : ICommand
    {
        private Vector3 _point;

        public MoveCommand(IMoveExecutor executor, Vector3 point) : base(executor)
        {
            _point = point;
        }

        public override CommandType GetCommandType() => CommandType.Move;

        public override void Execute()
        {
            (_executor as IMoveExecutor).Move(_point);
        }

        public override void Undo()
        {
            (_executor as IMoveExecutor).UnMove();
        }
    }
}
