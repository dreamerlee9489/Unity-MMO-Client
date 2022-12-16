using System.Collections.Generic;

namespace Manage
{
    public class MsgManager : BaseSingleton<MsgManager>
    {
        public delegate void MsgHandler(Google.Protobuf.IMessage msg);
        private readonly Dictionary<Net.MsgId, MsgHandler> _handlerDict = new();

        public void RegistMsgHandler(Net.MsgId msgId, MsgHandler handler)
        {
            if (!_handlerDict.ContainsKey(msgId))
                _handlerDict.Add(msgId, null);
            _handlerDict[msgId] += handler;
        }

        public void RemoveMsgHandler(Net.MsgId msgId, MsgHandler handler)
        {
            if (!_handlerDict.ContainsKey(msgId) && handler != null)
                _handlerDict[msgId] -= handler;
        }

        public void HandleMsg(Net.MsgId msgId, Google.Protobuf.IMessage msg)
        {
            if (_handlerDict.ContainsKey(msgId))
                _handlerDict[msgId]?.Invoke(msg);
        }
    }
}
