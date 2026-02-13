using ArtyEcs.Core;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class QueryBuilder
    {
        private List<ArchetypeMask> _masks = new();
        private int _masksUsed;
        private WorldInstance _world;
        private List<Entity> _entities = new(Constants.WORLD_ENTITIES_CAPACITY);

        public QueryBuilder(WorldInstance world)
        {
            _world = world;
        }

        public QueryBuilder With<T>()
        {
            AddMask<T>(1);
            return this;
        }

        public QueryBuilder Without<T>()
        {
            AddMask<T>(-1);
            return this;
        }

        public List<Entity> Execute()
        {
            _entities.Clear();
            var collectionWrapper = _world.GetAllEntities();
            for (int i = 0; i < collectionWrapper.Length; i++)
            {

                var entity = collectionWrapper.Collection[i];
                var compared = true;
                for (int j = 0; j < _masksUsed; j++)
                {
                    var mask = _masks[j];
                    var hasFlag = entity.Archetype.HasFlag(mask.Id);
                    if ((!hasFlag && mask.Value == 1)
                        || (hasFlag && mask.Value == -1))
                    {
                        compared = false;
                        break;
                    }
                }

                if (compared)
                {
                    _entities.Add(entity);
                }
            }

            return _entities;
        }

        internal void StartQuery()
        {
            _masksUsed = 0;
        }

        private void AddMask<T>(sbyte value)
        {
            var contextTypeId = ContextsManager.GetContextTypeId(typeof(T));
            if (_masks.Count <= _masksUsed)
            {
                _masks.Add(new ArchetypeMask { Id = contextTypeId, Value = value });
            }
            else
            {
                var mask = _masks[_masksUsed];
                mask.Id = contextTypeId;
                mask.Value = value;
            }
            _masksUsed++;
        }
    }
}
