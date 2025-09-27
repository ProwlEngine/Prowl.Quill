using System;
using System.Collections.Generic;
using Prowl.Quill.External.LibTessDotNet;

namespace Prowl.Quill
{
    public abstract class Poolable
    {
        public abstract void Reset();

        public virtual void OnFree()
        {
        }
    }

    public static class MemoryArena
    {
        private static Dictionary<Type, List<Poolable>> _arena = new Dictionary<Type, List<Poolable>>();
        private static Dictionary<Type, int> _indices = new Dictionary<Type, int>();

        public static void AddType<T>(int size = 0) where T : Poolable
        {
            _arena.TryAdd(typeof(T), new List<Poolable>(size));
            _indices.TryAdd(typeof(T), 0);
        }

        public static void AddTypes(Type[] typesToAdd)
        {
            foreach (Type type in typesToAdd)
            {
                if (!type.IsAssignableFrom(typeof(Poolable)))
                    throw new Exception($"{type} does not inherit from Poolabe and cannot be added!");

                _arena.TryAdd(type, new List<Poolable>());
                _indices.TryAdd(type, 0);
            }
        }

        public static T Get<T>() where T : Poolable, new()
        {
            if (!_arena.ContainsKey(typeof(T)))
                throw new Exception(
                    $"Dictionary does not contain the type {typeof(T)}, but you are trying to get an object!");

            List<Poolable> list = _arena[typeof(T)];
            int idx = _indices[typeof(T)];

            if (_indices[typeof(T)] >= list.Count)
            {
                list.Add(new T());
            }

            var obj = list[idx];
            obj.Reset();

            _indices[typeof(T)]++;

            return (T)obj;
        }

        public static void Free<T>()
        {
            if (!_arena.ContainsKey(typeof(T)))
                throw new Exception(
                    $"Dictionary does not contain the type {typeof(T)}, but you are trying to get an object!");

            _indices[typeof(T)] = 0;
        }

        public static void FreeTypes(Type[] typesToFree)
        {
            foreach (Type type in typesToFree)
            {
                _indices[type] = 0;
            }
        }

        public static void FreeAll()
        {
            foreach (Type type in _arena.Keys)
            {
                _indices[type] = 0;
            }
        }
    }
}