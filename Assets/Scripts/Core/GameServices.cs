using System;
using System.Collections.Generic;

/// <summary>
/// 轻量服务定位器 — 替代 FindAnyObjectByType
/// 组件在 Awake 注册，OnDestroy 注销
/// </summary>
public static class GameServices
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public static void Unregister<T>() where T : class
    {
        _services.Remove(typeof(T));
    }

    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out object service))
            return service as T;
        return null;
    }

    /// <summary>
    /// 清除所有注册（场景切换时调用）
    /// </summary>
    public static void Clear()
    {
        _services.Clear();
    }
}
