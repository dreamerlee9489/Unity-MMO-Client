using System;
using System.Collections.Generic;

namespace Manage
{
    public enum EventId
    {
        Connecting,
        Connected,
        Disconnect,

        HotUpdated,
        SceneUnload,
        SceneLoaded,

        PlayerLoaded,
    }

    interface IEventInfo { }

    class EventInfo : IEventInfo
    {
        public Action onEvent;
        public EventInfo(Action callback) { onEvent += callback; }
    }

    class EventInfo<T> : IEventInfo
    {
        public Action<T> onEvent;
        public EventInfo(Action<T> callback) { onEvent += callback; }
    }

    class EventInfo<T1, T2> : IEventInfo
    {
        public Action<T1, T2> onEvent;
        public EventInfo(Action<T1, T2> callback) { onEvent += callback; }
    }

    public class EventManager : BaseSingleton<EventManager>
    {
        private readonly Dictionary<EventId, List<IEventInfo>> _eventDict = new();

        public void AddListener(EventId type, Action callback)
        {
            if (!_eventDict.ContainsKey(type))
                _eventDict.Add(type, new List<IEventInfo>() { null, null, null });
            if (_eventDict[type][0] == null)
                _eventDict[type][0] = new EventInfo(null);
            (_eventDict[type][0] as EventInfo).onEvent += callback;
        }

        public void AddListener<T>(EventId type, Action<T> callback)
        {
            if (!_eventDict.ContainsKey(type))
                _eventDict.Add(type, new List<IEventInfo>() { null, null, null });
            if (_eventDict[type][1] == null)
                _eventDict[type][1] = new EventInfo<T>(null);
            (_eventDict[type][1] as EventInfo<T>).onEvent += callback;
        }

        public void AddListener<T1, T2>(EventId type, Action<T1, T2> callback)
        {
            if (!_eventDict.ContainsKey(type))
                _eventDict.Add(type, new List<IEventInfo>() { null, null, null });
            if (_eventDict[type][2] == null)
                _eventDict[type][2] = new EventInfo<T1, T2>(null);
            (_eventDict[type][2] as EventInfo<T1, T2>).onEvent += callback;
        }

        public void RemoveListener(EventId type, Action callback)
        {
            if (_eventDict.ContainsKey(type))
                (_eventDict[type][0] as EventInfo).onEvent -= callback;
        }

        public void RemoveListener<T>(EventId type, Action<T> callback)
        {
            if (_eventDict.ContainsKey(type))
                (_eventDict[type][1] as EventInfo<T>).onEvent -= callback;
        }

        public void RemoveListener<T1, T2>(EventId type, Action<T1, T2> callback)
        {
            if (_eventDict.ContainsKey(type))
                (_eventDict[type][2] as EventInfo<T1, T2>).onEvent -= callback;
        }

        public void Invoke(EventId type)
        {
            if (_eventDict.ContainsKey(type))
                (_eventDict[type][0] as EventInfo).onEvent?.Invoke();
        }

        public void Invoke<T>(EventId type, T arg)
        {
            if (_eventDict.ContainsKey(type))
                (_eventDict[type][1] as EventInfo<T>).onEvent?.Invoke(arg);
        }

        public void Invoke<T1, T2>(EventId type, T1 arg1, T2 arg2)
        {
            if (_eventDict.ContainsKey(type))
                (_eventDict[type][2] as EventInfo<T1, T2>).onEvent?.Invoke(arg1, arg2);
        }
    }
}
