using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public struct ModifiableComponentCollection<T> : IDisposable where T : struct, IComponent
    {
        private readonly ComponentTable<T> _storage;
        private readonly World _world;
        private T[] _modifiableComponents;
        private HashSet<int> _modifiedIndices;
        private bool _disposed;
        private bool _applied;

        internal ModifiableComponentCollection(ComponentTable<T> storage, World world)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _world = world;
            _disposed = false;
            _applied = false;

            var count = storage.Count;
            if (count > 0)
            {
                var (components, _, _) = storage.GetInternalTable();
                _modifiableComponents = new T[count];
                Array.Copy(components, _modifiableComponents, count);
                _modifiedIndices = new HashSet<int>();
            }
            else
            {
                _modifiableComponents = Array.Empty<T>();
                _modifiedIndices = new HashSet<int>();
            }
        }

        public int Count => _modifiableComponents?.Length ?? 0;

        public ref T this[int index]
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ModifiableComponentCollection<T>));

                if (index < 0 || index >= _modifiableComponents.Length)
                    throw new IndexOutOfRangeException($"Index {index} is out of range [0, {_modifiableComponents.Length})");

                _modifiedIndices.Add(index);

                return ref _modifiableComponents[index];
            }
        }

        public Entity GetEntity(int index)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModifiableComponentCollection<T>));

            return _storage.GetEntityAt(index);
        }

        public void Apply()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModifiableComponentCollection<T>));

            if (_applied)
                return;

            if (_modifiableComponents == null || _modifiedIndices == null || _modifiedIndices.Count == 0)
            {
                _applied = true;
                return;
            }

            int modifiedCount = _modifiedIndices.Count;
            int[] sortedIndices = new int[modifiedCount];
            int i = 0;
            foreach (int index in _modifiedIndices)
            {
                sortedIndices[i++] = index;
            }
            
            Array.Sort(sortedIndices);

            var (components, _, _) = _storage.GetInternalTable();
            int componentsLength = components.Length;
            int modifiableLength = _modifiableComponents.Length;
            
            for (int j = 0; j < sortedIndices.Length; j++)
            {
                int index = sortedIndices[j];
                if (index < componentsLength && index < modifiableLength)
                {
                    components[index] = _modifiableComponents[index];
                }
            }

            _applied = true;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (!_applied)
                {
                    Apply();
                }
                
                _disposed = true;
            }
        }
    }
}

