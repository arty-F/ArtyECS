using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class QueryBuilder
    {
        private List<int> _withTypeIds = new();
        private List<int> _withoutTypeIds = new();
        private WorldInstance _world;

        public QueryBuilder(WorldInstance world)
        {
            _world = world;
        }

        public QueryBuilder With<T>()
        {
            AddTypeId<T>(_withTypeIds);
            return this;
        }

        public QueryBuilder Without<T>()
        {
            AddTypeId<T>(_withoutTypeIds);
            return this;
        }

        public IEnumerable<Entity> Execute()
        {
            var withArchetype = ComponentsManager.GetArchetype(_withTypeIds);
            
            foreach (var entity in _world.GetAllEntities(withArchetype))
            {
                bool hasExcludedComponent = false;
                
                for (int i = 0; i < _withoutTypeIds.Count; i++)
                {
                    if (entity.Archetype.HasBit(_withoutTypeIds[i]))
                    {
                        hasExcludedComponent = true;
                        break;
                    }
                }
                
                if (!hasExcludedComponent)
                {
                    yield return entity;
                }
            }
        }

        internal void StartQuery()
        {
            _withTypeIds.Clear();
            _withoutTypeIds.Clear();
        }

        private void AddTypeId<T>(List<int> collection)
        {
            var componentTypeId = ComponentsManager.GetComponentTypeId(typeof(T));
            collection.Add(componentTypeId);
        }
    }
}
