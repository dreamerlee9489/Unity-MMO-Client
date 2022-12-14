﻿using System.Collections.Generic;
using UnityEngine;

namespace Manage
{
    public class MsgManager : BaseSingleton<MsgManager>
    {
        public delegate void MsgHandler(Google.Protobuf.IMessage msg);
        private readonly Dictionary<Proto.MsgId, MsgHandler> _handlerDict = new();

        public void RegistMsgHandler(Proto.MsgId msgId, MsgHandler handler)
        {
            if (!_handlerDict.ContainsKey(msgId))
                _handlerDict.Add(msgId, null);
            _handlerDict[msgId] += handler;
        }

        public void RemoveMsgHandler(Proto.MsgId msgId, MsgHandler handler)
        {
            if (!_handlerDict.ContainsKey(msgId) && handler != null)
                _handlerDict[msgId] -= handler;
        }

        public void HandleMsg(Proto.MsgId msgId, Google.Protobuf.IMessage msg)
        {
            if (!_handlerDict.ContainsKey(msgId))
            {
                Debug.Log("未处理消息 msgid=" + msgId);
                return;
            }
            _handlerDict[msgId]?.Invoke(msg);
        }
    }
}