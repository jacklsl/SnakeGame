using System;
using System.Collections.Generic;

/// <summary>
/// 全局事件总线 — 订阅/发布 struct 事件，零 GC 分配
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new();

    public static void Subscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        if (_handlers.ContainsKey(type))
            _handlers[type] = Delegate.Combine(_handlers[type], handler);
        else
            _handlers[type] = handler;
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        if (!_handlers.ContainsKey(type))
            return;

        Delegate combined = Delegate.Remove(_handlers[type], handler);
        if (combined == null)
            _handlers.Remove(type);
        else
            _handlers[type] = combined;
    }

    public static void Publish<T>(T evt) where T : struct
    {
        Type type = typeof(T);
        if (_handlers.TryGetValue(type, out Delegate del) && del is Action<T> action)
            action.Invoke(evt);
    }

    /// <summary>
    /// 清除所有订阅（场景切换时调用）
    /// </summary>
    public static void Clear()
    {
        _handlers.Clear();
    }
}
