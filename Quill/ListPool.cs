using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public static class ListPool<T>
{
    private static readonly List<List<T>> _pool = new List<List<T>>();
    private static int _poolIndex = 0;

    public static List<T> Rent()
    {
        if (_poolIndex >= _pool.Count)
        {
            _pool.Add(new List<T>());
        }

        var list = _pool[_poolIndex];
        _poolIndex++;
        return list;
    }

    public static void Return(List<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        list.Clear();
    }

    public static void Free()
    {
        _poolIndex = 0;
    }
}