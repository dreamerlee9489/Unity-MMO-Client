using Manage;
using System;
using UnityEngine;

public abstract class GuidObject : MonoBehaviour
{
    protected string _guid;
    protected static WorldManager _currWorld;

    public string GetGuid() => _guid;

    protected virtual void Awake()
    {
        _guid = Guid.NewGuid().ToString();
        EventManager.Instance.AddListener(EEventType.SceneUnload, SceneUnloadCallback);
        EventManager.Instance.AddListener(EEventType.SceneLoaded, SceneLoadedCallback);
    }

    protected virtual void Start()
    {
        if (_currWorld)
            _currWorld.inWorldObjDict.TryAdd(_guid, transform);
    }

    protected virtual void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EEventType.SceneUnload, SceneUnloadCallback);
        EventManager.Instance.RemoveListener(EEventType.SceneLoaded, SceneLoadedCallback);
    }

    protected void SceneUnloadCallback()
    {
        if (_currWorld)
            _currWorld.inWorldObjDict.Clear();
        _currWorld = null;
    }

    protected void SceneLoadedCallback()
    {
        if (_currWorld == null)
            _currWorld = FindObjectOfType<WorldManager>();
        _currWorld.inWorldObjDict.TryAdd(_guid, transform);
    }
}
