using ArtyEcs.Core;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class QueryBuilder
    {
        private List<ArchetypeMask> _masks = new();
        private int _masksUsed;
        private WorldInstance _world;
        private List<Entity> _entities = new();

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
            var entites = _world.GetAllEntities();
            for (int i = 0; i < entites.Count; i++)
            {
                var entity = entites[i];
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
            var componentTypeId = ComponentsManager.GetComponentTypeId(typeof(T));
            if (_masks.Count <= _masksUsed)
            {
                _masks.Add(new ArchetypeMask { Id = componentTypeId, Value = value });
            }
            else
            {
                var mask = _masks[_masksUsed];
                mask.Id = componentTypeId;
                mask.Value = value;
            }
            _masksUsed++;
        }
    }
}
