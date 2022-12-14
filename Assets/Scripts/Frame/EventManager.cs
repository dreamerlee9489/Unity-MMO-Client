using System;
using System.Collections.Generic;

namespace Frame
{
    public enum EEventType
    {
        Connecting,
        Connected,
        Disconnect,

        SceneLoading,
        SceneLoaded,
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
        Dictionary<EEventType, List<IEventInfo>> eventDict = new();

        public void AddListener(EEventType type, Action callback)
        {
            if (!eventDict.ContainsKey(type))
                eventDict.Add(type, new List<IEventInfo>() { null, null, null });
            if (eventDict[type][0] == null)
                eventDict[type][0] = new EventInfo(null);
            (eventDict[type][0] as EventInfo).onEvent += callback;
        }

        public void AddListener<T>(EEventType type, Action<T> callback)
        {
            if (!eventDict.ContainsKey(type))
                eventDict.Add(type, new List<IEventInfo>() { null, null, null });
            if (eventDict[type][1] == null)
                eventDict[type][1] = new EventInfo<T>(null);
            (eventDict[type][1] as EventInfo<T>).onEvent += callback;
        }

        public void AddListener<T1, T2>(EEventType type, Action<T1, T2> callback)
        {
            if (!eventDict.ContainsKey(type))
                eventDict.Add(type, new List<IEventInfo>() { null, null, null });
            if (eventDict[type][2] == null)
                eventDict[type][2] = new EventInfo<T1, T2>(null);
            (eventDict[type][2] as EventInfo<T1, T2>).onEvent += callback;
        }

        public void RemoveListener(EEventType type, Action callback)
        {
            if (eventDict.ContainsKey(type))
                (eventDict[type][0] as EventInfo).onEvent -= callback;
        }

        public void RemoveListener<T>(EEventType type, Action<T> callback)
        {
            if (eventDict.ContainsKey(type))
                (eventDict[type][1] as EventInfo<T>).onEvent -= callback;
        }

        public void RemoveListener<T1, T2>(EEventType type, Action<T1, T2> callback)
        {
            if (eventDict.ContainsKey(type))
                (eventDict[type][2] as EventInfo<T1, T2>).onEvent -= callback;
        }

        public void Invoke(EEventType type)
        {
            if (eventDict.ContainsKey(type))
                (eventDict[type][0] as EventInfo).onEvent?.Invoke();
        }

        public void Invoke<T>(EEventType type, T arg)
        {
            if (eventDict.ContainsKey(type))
                (eventDict[type][1] as EventInfo<T>).onEvent?.Invoke(arg);
        }

        public void Invoke<T1, T2>(EEventType type, T1 arg1, T2 arg2)
        {
            if (eventDict.ContainsKey(type))
                (eventDict[type][2] as EventInfo<T1, T2>).onEvent?.Invoke(arg1, arg2);
        }
    }
}
