using UnityEngine;

namespace Control.CMD
{
    public interface IMoveExecutor : IExecutor
    {
        void Move(Vector3 point);

        void UnMove();
    }
}