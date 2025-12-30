using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public struct QueryBuilder
    {
        private readonly WorldInstance _world;
        private readonly HashSet<Type> _withTypes;
        private readonly HashSet<Type> _withoutTypes;

        internal QueryBuilder(WorldInstance world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _withTypes = new HashSet<Type>();
            _withoutTypes = new HashSet<Type>();
        }

        private QueryBuilder(WorldInstance world, HashSet<Type> withTypes, HashSet<Type> withoutTypes)
        {
            _world = world;
            _withTypes = withTypes;
            _withoutTypes = withoutTypes;
        }

        public QueryBuilder With<T>() where T : struct, IComponent
        {
            var newWithTypes = _withTypes == null ? new HashSet<Type>() : new HashSet<Type>(_withTypes);
            newWithTypes.Add(typeof(T));
            
            var newWithoutTypes = _withoutTypes == null ? new HashSet<Type>() : new HashSet<Type>(_withoutTypes);
            
            return new QueryBuilder(_world, newWithTypes, newWithoutTypes);
        }

        public QueryBuilder Without<T>() where T : struct, IComponent
        {
            var newWithTypes = _withTypes == null ? new HashSet<Type>() : new HashSet<Type>(_withTypes);
            
            var newWithoutTypes = _withoutTypes == null ? new HashSet<Type>() : new HashSet<Type>(_withoutTypes);
            newWithoutTypes.Add(typeof(T));
            
            return new QueryBuilder(_world, newWithTypes, newWithoutTypes);
        }

        public ReadOnlySpan<Entity> Execute()
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.QueryBuilderExecute, _world, BuildComponentTypesString()))
#endif
            {
                if (_withTypes == null || _withTypes.Count == 0)
                {
                    if (_withoutTypes == null || _withoutTypes.Count == 0)
                    {
                        return ReadOnlySpan<Entity>.Empty;
                    }

                    return ExecuteWithoutOnly();
                }
                else if (_withoutTypes == null || _withoutTypes.Count == 0)
                {
                    return ExecuteWithOnly();
                }
                else
                {
                    return ExecuteWithAndWithout();
                }
            }
        }

#if UNITY_EDITOR
        private string BuildComponentTypesString()
        {
            var parts = new List<string>();
            
            if (_withTypes != null && _withTypes.Count > 0)
            {
                foreach (var type in _withTypes)
                {
                    parts.Add(type.Name);
                }
            }
            
            if (_withoutTypes != null && _withoutTypes.Count > 0)
            {
                foreach (var type in _withoutTypes)
                {
                    parts.Add($"!{type.Name}");
                }
            }
            
            return parts.Count > 0 ? string.Join(", ", parts) : null;
        }
#endif

        private ReadOnlySpan<Entity> ExecuteWithOnly()
        {
            PooledHashSet<Entity> resultSet = default;
            ReadOnlySpan<Entity> baseEntities = ReadOnlySpan<Entity>.Empty;
            bool firstSet = true;

            foreach (var type in _withTypes)
            {
                var entities = GetEntitiesWithType(type);
                
                if (entities.Length == 0)
                {
                    if (resultSet.Set != null)
                        resultSet.Dispose();
                    return ReadOnlySpan<Entity>.Empty;
                }

                if (firstSet)
                {
                    resultSet = EntityHashSetPool.RentPooled(entities.Length);
                    foreach (var entity in entities)
                    {
                        resultSet.Set.Add(entity);
                    }
                    baseEntities = entities;
                    firstSet = false;
                }
                else
                {
                    using var newSet = EntityHashSetPool.RentPooled(entities.Length);
                    foreach (var entity in entities)
                    {
                        newSet.Set.Add(entity);
                    }
                    resultSet.Set.IntersectWith(newSet.Set);
                    
                    if (resultSet.Set.Count == 0)
                    {
                        resultSet.Dispose();
                        return ReadOnlySpan<Entity>.Empty;
                    }
                }
            }

            if (resultSet.Set == null || resultSet.Set.Count == 0)
            {
                if (resultSet.Set != null)
                    resultSet.Dispose();
                return ReadOnlySpan<Entity>.Empty;
            }

            int resultCount = resultSet.Set.Count;
            var resultArray = QueryContext.Get(_world).RentArray(resultCount);
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (resultSet.Set.Contains(entity))
                {
                    resultArray[index++] = entity;
                }
            }

            resultSet.Dispose();
            return new ReadOnlySpan<Entity>(resultArray, 0, resultCount);
        }

        private ReadOnlySpan<Entity> ExecuteWithoutOnly()
        {
            var allEntities = GetAllEntitiesInWorld();
            
            if (allEntities.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            foreach (var type in _withoutTypes)
            {
                using var exclusionSet = GetEntitiesWithTypeAsSetPooled(type);
                allEntities.ExceptWith(exclusionSet.Set);
                
                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }
            }

            var resultArray = QueryContext.Get(_world).RentArray(allEntities.Count);
            int index = 0;
            foreach (var entity in allEntities)
            {
                resultArray[index++] = entity;
            }

            return new ReadOnlySpan<Entity>(resultArray, 0, allEntities.Count);
        }

        private ReadOnlySpan<Entity> ExecuteWithAndWithout()
        {
            PooledHashSet<Entity> resultSet = default;
            ReadOnlySpan<Entity> baseEntities = ReadOnlySpan<Entity>.Empty;
            bool firstSet = true;

            foreach (var type in _withTypes)
            {
                var entities = GetEntitiesWithType(type);
                
                if (entities.Length == 0)
                {
                    if (resultSet.Set != null)
                        resultSet.Dispose();
                    return ReadOnlySpan<Entity>.Empty;
                }

                if (firstSet)
                {
                    resultSet = EntityHashSetPool.RentPooled(entities.Length);
                    foreach (var entity in entities)
                    {
                        resultSet.Set.Add(entity);
                    }
                    baseEntities = entities;
                    firstSet = false;
                }
                else
                {
                    using var newSet = EntityHashSetPool.RentPooled(entities.Length);
                    foreach (var entity in entities)
                    {
                        newSet.Set.Add(entity);
                    }
                    resultSet.Set.IntersectWith(newSet.Set);
                    
                    if (resultSet.Set.Count == 0)
                    {
                        resultSet.Dispose();
                        return ReadOnlySpan<Entity>.Empty;
                    }
                }
            }

            if (resultSet.Set == null || resultSet.Set.Count == 0)
            {
                if (resultSet.Set != null)
                    resultSet.Dispose();
                return ReadOnlySpan<Entity>.Empty;
            }

            foreach (var type in _withoutTypes)
            {
                using var exclusionSet = GetEntitiesWithTypeAsSetPooled(type);
                resultSet.Set.ExceptWith(exclusionSet.Set);
                
                if (resultSet.Set.Count == 0)
                {
                    resultSet.Dispose();
                    return ReadOnlySpan<Entity>.Empty;
                }
            }

            int resultCount = resultSet.Set.Count;
            var resultArray = QueryContext.Get(_world).RentArray(resultCount);
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (resultSet.Set.Contains(entity))
                {
                    resultArray[index++] = entity;
                }
            }

            resultSet.Dispose();
            return new ReadOnlySpan<Entity>(resultArray, 0, resultCount);
        }

        private ReadOnlySpan<Entity> GetEntitiesWithType(Type componentType)
        {
            var table = ComponentsManager.GetTableByType(componentType, _world);
            if (table == null)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            return table.GetEntities();
        }

        private HashSet<Entity> GetEntitiesWithTypeAsSet(Type componentType)
        {
            var table = ComponentsManager.GetTableByType(componentType, _world);
            if (table == null)
            {
                return new HashSet<Entity>();
            }

            var entities = table.GetEntities();
            var set = new HashSet<Entity>(entities.Length);
            foreach (var entity in entities)
            {
                set.Add(entity);
            }
            return set;
        }

        private PooledHashSet<Entity> GetEntitiesWithTypeAsSetPooled(Type componentType)
        {
            var table = ComponentsManager.GetTableByType(componentType, _world);
            if (table == null)
            {
                return EntityHashSetPool.RentPooled();
            }

            var entities = table.GetEntities();
            var set = EntityHashSetPool.RentPooled(entities.Length);
            foreach (var entity in entities)
            {
                set.Set.Add(entity);
            }
            return set;
        }

        private HashSet<Entity> GetAllEntitiesInWorld()
        {
            return ComponentsManager.GetAllEntitiesInWorld(_world);
        }
    }
}
