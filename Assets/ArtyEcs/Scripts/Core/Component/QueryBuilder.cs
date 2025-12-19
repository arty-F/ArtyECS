using System;
using System.Collections.Generic;
using System.Reflection;

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
            if (_withTypes == null || _withTypes.Count == 0)
            {
                if (_withoutTypes == null || _withoutTypes.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                return ExecuteWithoutOnly();
            }

            if (_withoutTypes == null || _withoutTypes.Count == 0)
            {
                return ExecuteWithOnly();
            }

            return ExecuteWithAndWithout();
        }

        private ReadOnlySpan<Entity> ExecuteWithOnly()
        {
            HashSet<Entity> resultSet = null;
            ReadOnlySpan<Entity> baseEntities = ReadOnlySpan<Entity>.Empty;

            foreach (var type in _withTypes)
            {
                var entities = GetEntitiesWithType(type);
                
                if (entities.Length == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                if (resultSet == null)
                {
                    resultSet = new HashSet<Entity>(entities.Length);
                    foreach (var entity in entities)
                    {
                        resultSet.Add(entity);
                    }
                    baseEntities = entities;
                }
                else
                {
                    var newSet = new HashSet<Entity>(entities.Length);
                    foreach (var entity in entities)
                    {
                        newSet.Add(entity);
                    }
                    resultSet.IntersectWith(newSet);
                    
                    if (resultSet.Count == 0)
                    {
                        return ReadOnlySpan<Entity>.Empty;
                    }
                }
            }

            if (resultSet == null || resultSet.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[resultSet.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (resultSet.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
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
                var exclusionSet = GetEntitiesWithTypeAsSet(type);
                allEntities.ExceptWith(exclusionSet);
                
                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }
            }

            var result = new Entity[allEntities.Count];
            int index = 0;
            foreach (var entity in allEntities)
            {
                result[index++] = entity;
            }

            return result;
        }

        private ReadOnlySpan<Entity> ExecuteWithAndWithout()
        {
            HashSet<Entity> resultSet = null;
            ReadOnlySpan<Entity> baseEntities = ReadOnlySpan<Entity>.Empty;

            foreach (var type in _withTypes)
            {
                var entities = GetEntitiesWithType(type);
                
                if (entities.Length == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                if (resultSet == null)
                {
                    resultSet = new HashSet<Entity>(entities.Length);
                    foreach (var entity in entities)
                    {
                        resultSet.Add(entity);
                    }
                    baseEntities = entities;
                }
                else
                {
                    var newSet = new HashSet<Entity>(entities.Length);
                    foreach (var entity in entities)
                    {
                        newSet.Add(entity);
                    }
                    resultSet.IntersectWith(newSet);
                    
                    if (resultSet.Count == 0)
                    {
                        return ReadOnlySpan<Entity>.Empty;
                    }
                }
            }

            if (resultSet == null || resultSet.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            foreach (var type in _withoutTypes)
            {
                var exclusionSet = GetEntitiesWithTypeAsSet(type);
                resultSet.ExceptWith(exclusionSet);
                
                if (resultSet.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }
            }

            var result = new Entity[resultSet.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (resultSet.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }

        private ReadOnlySpan<Entity> GetEntitiesWithType(Type componentType)
        {
            var method = typeof(WorldInstance).GetMethod(
                nameof(WorldInstance.GetEntitiesWith),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);

            if (method == null)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var genericMethod = method.MakeGenericMethod(componentType);
            var result = genericMethod.Invoke(_world, null);
            
            return result is ReadOnlySpan<Entity> span ? span : ReadOnlySpan<Entity>.Empty;
        }

        private HashSet<Entity> GetEntitiesWithTypeAsSet(Type componentType)
        {
            var entities = GetEntitiesWithType(componentType);
            var set = new HashSet<Entity>(entities.Length);
            foreach (var entity in entities)
            {
                set.Add(entity);
            }
            return set;
        }

        private HashSet<Entity> GetAllEntitiesInWorld()
        {
            var allEntities = new HashSet<Entity>();

            var method = typeof(ComponentsManager).GetMethod(
                "GetAllEntitiesInWorld",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (method == null)
            {
                return allEntities;
            }

            var result = method.Invoke(null, new object[] { _world });
            
            return result is HashSet<Entity> set ? set : allEntities;
        }
    }
}
