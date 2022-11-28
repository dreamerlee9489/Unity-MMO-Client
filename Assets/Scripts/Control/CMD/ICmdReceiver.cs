using UnityEngine;

namespace Control.CMD
{
    public interface ICmdReceiver
    {
        void Execute(Vector3 point);
        void Execute(Transform transform);
        void Undo();
    }
}