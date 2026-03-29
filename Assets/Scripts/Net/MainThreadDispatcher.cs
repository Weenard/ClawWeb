using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> _queue = new();
    private static MainThreadDispatcher _instance;

    public static void EnsureExists()
    {
        if (_instance != null) return;
        var go = new GameObject("[MainThreadDispatcher]");
        _instance = go.AddComponent<MainThreadDispatcher>();
        DontDestroyOnLoad(go);
    }

    public static void Enqueue(Action a)
    {
        if (a == null) return;
        _queue.Enqueue(a);
    }

    private void Update()
    {
        while (_queue.TryDequeue(out var a))
            a?.Invoke();
    }
}
