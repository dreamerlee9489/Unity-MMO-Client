using UnityEngine;

namespace Control.CMD
{
    public interface IMover : IExecutor
    {
        void Move(Vector3 point);
        void UnMove();
    }

    public class MoveCommand : ICommand
    {
        private Vector3 _point;

        public MoveCommand(IMover executor, Vector3 point) : base(executor)
        {
            _point = point;
            (_executor as GameEntity).Target = null;
            (_executor as IMover).Move(_point);
        }

        public override CommandType GetCmdType() => CommandType.Move;

        public override void Execute()
        {
            (_executor as IMover).Move(_point);
        }

        public override void Undo()
        {
            (_executor as IMover).UnMove();
        }
    } 
}
