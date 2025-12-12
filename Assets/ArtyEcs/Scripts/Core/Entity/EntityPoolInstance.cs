using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal class EntityPoolInstance
    {
        private const int DefaultInitialCapacity = 256;

        private readonly Stack<int> _availableIds;

        private readonly Dictionary<int, int> _generations;

        private int _allocatedCount;

        private readonly int _initialCapacity;

        private bool _preAllocated;

        private int _unusedPreAllocatedCount;

        internal EntityPoolInstance(int initialCapacity, ref int globalNextId)
        {
            _availableIds = new Stack<int>(initialCapacity);
            _generations = new Dictionary<int, int>(initialCapacity);
            _initialCapacity = initialCapacity;
            _preAllocated = false;
            _unusedPreAllocatedCount = 0;
            _allocatedCount = 0;
        }

        internal Entity Allocate(ref int globalNextId)
        {
            if (!_preAllocated)
            {
                for (int i = 0; i < _initialCapacity; i++)
                {
                    int preAllocatedId = globalNextId++;
                    _availableIds.Push(preAllocatedId);
                    _generations[preAllocatedId] = 0;
                }
                _preAllocated = true;
                _unusedPreAllocatedCount = _initialCapacity;
            }

            int id;
            int generation;

            if (_availableIds.Count > 0)
            {
                id = _availableIds.Pop();
                
                if (_unusedPreAllocatedCount > 0)
                {
                    _unusedPreAllocatedCount--;
                }
                
                if (_generations.TryGetValue(id, out int currentGen))
                {
                    generation = currentGen + 1;
                }
                else
                {
                    generation = 0;
                }
                
                _generations[id] = generation;
            }
            else
            {
                id = globalNextId++;
                generation = 0;
                _generations[id] = generation;
            }

            _allocatedCount++;
            return new Entity(id, generation);
        }

        internal bool Deallocate(Entity entity)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            int id = entity.Id;
            int generation = entity.Generation;

            if (!_generations.TryGetValue(id, out int currentGen) || currentGen != generation)
            {
                return false;
            }

            _generations[id] = generation + 1;

            _availableIds.Push(id);

            _allocatedCount--;
            return true;
        }

        internal bool IsAllocated(Entity entity)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            int id = entity.Id;
            int generation = entity.Generation;

            return _generations.TryGetValue(id, out int currentGen) && currentGen == generation;
        }

        internal int GetAllocatedCount()
        {
            return _allocatedCount;
        }

        internal int GetAvailableCount()
        {
            return _availableIds.Count - _unusedPreAllocatedCount;
        }

        internal void Clear()
        {
            _availableIds.Clear();
            _generations.Clear();
            _allocatedCount = 0;
            _preAllocated = false;
            _unusedPreAllocatedCount = 0;
        }
    }
}

