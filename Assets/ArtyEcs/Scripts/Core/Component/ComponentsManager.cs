using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class ComponentsManager
    {
        private static readonly Dictionary<WorldInstance, Dictionary<Type, IComponentTable>> WorldTables =
            new Dictionary<WorldInstance, Dictionary<Type, IComponentTable>>();

        private static readonly Dictionary<(WorldInstance world, Type type), IComponentTable> TableCache =
            new Dictionary<(WorldInstance world, Type type), IComponentTable>();

        private static Dictionary<Type, IComponentTable> GetWorldTable(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!WorldTables.TryGetValue(world, out var table))
            {
                table = new Dictionary<Type, IComponentTable>();
                WorldTables[world] = table;
            }

            return table;
        }

        private static void ValidateEntityForRead(Entity entity, WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!entity.IsValid)
            {
                throw new InvalidEntityException(entity);
            }
        }

        private static void ValidateEntityForWrite(Entity entity, WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!entity.IsValid)
            {
                throw new InvalidEntityException(entity);
            }

            if (!EntitiesManager.IsAllocated(entity, world))
            {
                throw new InvalidEntityException(entity);
            }
        }

        internal static ComponentTable<T> GetOrCreateTable<T>(WorldInstance world) where T : struct, IComponent
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            Type componentType = typeof(T);
            var cacheKey = (world, componentType);

            if (TableCache.TryGetValue(cacheKey, out var cachedTable))
            {
                return (ComponentTable<T>)cachedTable;
            }

            var worldTable = GetWorldTable(world);

            if (!worldTable.TryGetValue(componentType, out var table))
            {
                table = new ComponentTable<T>();
                worldTable[componentType] = table;
            }

            TableCache[cacheKey] = table;

            return (ComponentTable<T>)table;
        }

        internal static IComponentTable GetTableByType(Type componentType, WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (componentType == null)
                throw new ArgumentNullException(nameof(componentType));

            if (!WorldTables.TryGetValue(world, out var worldTable))
            {
                return null;
            }

            if (worldTable.TryGetValue(componentType, out var table))
            {
                return table;
            }

            return null;
        }

        internal static int GetWorldCount()
        {
            return WorldTables.Count;
        }

        internal static bool IsWorldInitialized(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            return WorldTables.ContainsKey(world);
        }

        internal static void AddComponent<T>(Entity entity, T component, WorldInstance world) where T : struct, IComponent
        {
            ValidateEntityForWrite(entity, world);

            var table = GetOrCreateTable<T>(world);

            if (table.HasComponent(entity))
            {
                throw new DuplicateComponentException(entity, typeof(T));
            }

            ((IComponentTable)table).AddComponentForEntity(entity, component);
        }

        internal static bool RemoveComponent<T>(Entity entity, WorldInstance world) where T : struct, IComponent
        {
            ValidateEntityForWrite(entity, world);

            var table = GetOrCreateTable<T>(world);

            if (!table.HasComponent(entity))
            {
                return false;
            }

            table.RemoveComponentInternal(entity);
            return true;
        }

        internal static T GetComponent<T>(Entity entity, WorldInstance world) where T : struct, IComponent
        {
            ValidateEntityForRead(entity, world);

            var table = GetOrCreateTable<T>(world);

            if (table.TryGetComponent(entity, out T component))
            {
                return component;
            }

            throw new ComponentNotFoundException(entity, typeof(T));
        }

        internal static bool HasComponent<T>(Entity entity, WorldInstance world) where T : struct, IComponent
        {
            ValidateEntityForRead(entity, world);

            var table = GetOrCreateTable<T>(world);
            return table.HasComponent(entity);
        }

        internal static ref T GetModifiableComponent<T>(Entity entity, WorldInstance world) where T : struct, IComponent
        {
            ValidateEntityForRead(entity, world);

            var table = GetOrCreateTable<T>(world);
            
            return ref table.GetModifiableComponentRef(entity);
        }

        internal static ReadOnlySpan<T> GetComponents<T>(WorldInstance world) where T : struct, IComponent
        {
            var table = GetOrCreateTable<T>(world);

            return table.GetComponents();
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1>(WorldInstance world) where T1 : struct, IComponent
        {
            var table = GetOrCreateTable<T1>(world);
            return table.GetEntities();
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);

            if (table1.Count == 0 || table2.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;
            
            if (table1.Count <= table2.Count)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                var set2 = table2.GetEntitiesSet();
                intersection.IntersectWith(set2);
            }
            else
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                var set1 = table1.GetEntitiesSet();
                intersection.IntersectWith(set1);
            }

            if (intersection.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[intersection.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (intersection.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);

            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            int minCount = Math.Min(Math.Min(table1.Count, table2.Count), table3.Count);
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
            }
            else
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
            }

            if (intersection.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[intersection.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (intersection.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }

        internal static HashSet<Entity> GetAllEntitiesInWorld(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            var allEntities = new HashSet<Entity>();

            if (!WorldTables.TryGetValue(world, out var worldTable))
            {
                return allEntities;
            }

            foreach (var table in worldTable.Values)
            {
                var entities = table.GetEntities();
                foreach (var entity in entities)
                {
                    allEntities.Add(entity);
                }
            }

            return allEntities;
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWithout<T1>(WorldInstance world) where T1 : struct, IComponent
        {
            var allEntities = GetAllEntitiesInWorld(world);
            
            if (allEntities.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var table1 = GetOrCreateTable<T1>(world);
            var exclusionSet = table1.GetEntitiesSet();
            
            allEntities.ExceptWith(exclusionSet);

            if (allEntities.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[allEntities.Count];
            int index = 0;
            foreach (var entity in allEntities)
            {
                result[index++] = entity;
            }

            return result;
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            var allEntities = GetAllEntitiesInWorld(world);
            
            if (allEntities.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            
            var exclusionSet = table1.GetEntitiesSet();
            exclusionSet.UnionWith(table2.GetEntitiesSet());
            
            allEntities.ExceptWith(exclusionSet);

            if (allEntities.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[allEntities.Count];
            int index = 0;
            foreach (var entity in allEntities)
            {
                result[index++] = entity;
            }

            return result;
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var allEntities = GetAllEntitiesInWorld(world);
            
            if (allEntities.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);
            
            var exclusionSet = table1.GetEntitiesSet();
            exclusionSet.UnionWith(table2.GetEntitiesSet());
            exclusionSet.UnionWith(table3.GetEntitiesSet());
            
            allEntities.ExceptWith(exclusionSet);

            if (allEntities.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[allEntities.Count];
            int index = 0;
            foreach (var entity in allEntities)
            {
                result[index++] = entity;
            }

            return result;
        }

        internal static ModifiableComponentCollection<T> GetModifiableComponents<T>(WorldInstance world) where T : struct, IComponent
        {
            var table = GetOrCreateTable<T>(world);
            return new ModifiableComponentCollection<T>(table);
        }

        internal static int RemoveAllComponents(Entity entity, WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            int removedCount = 0;

            if (!WorldTables.TryGetValue(world, out var worldTable))
            {
                return 0;
            }

            foreach (var table in worldTable.Values)
            {
                if (table.TryRemoveComponentForEntity(entity))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        internal static void ClearWorld(WorldInstance world)
        {
            if (world == null)
            {
                return;
            }

            var keysToRemove = new List<(WorldInstance world, Type type)>();
            foreach (var cacheKey in TableCache.Keys)
            {
                if (cacheKey.world.Equals(world))
                {
                    keysToRemove.Add(cacheKey);
                }
            }

            foreach (var key in keysToRemove)
            {
                TableCache.Remove(key);
            }

            WorldTables.Remove(world);
        }

        internal static void ClearAll()
        {
            TableCache.Clear();
            WorldTables.Clear();
        }

        internal static ComponentInfo[] GetAllComponentInfos(Entity entity, WorldInstance world)
        {
            ValidateEntityForRead(entity, world);

            if (!WorldTables.TryGetValue(world, out var worldTable))
            {
                return Array.Empty<ComponentInfo>();
            }

            var componentInfos = new List<ComponentInfo>();

            foreach (var kvp in worldTable)
            {
                var componentType = kvp.Key;
                var table = kvp.Value;

                if (table.HasComponentForEntity(entity))
                {
                    var componentValue = table.GetComponentValue(entity);
                    var jsonValue = GetJsonValue(componentValue);

                    componentInfos.Add(new ComponentInfo(componentType, componentValue, jsonValue));
                }
            }

            return componentInfos.ToArray();
        }

        private static string GetJsonValue(object value)
        {
            if (value == null)
                return null;

            return UnityEngine.JsonUtility.ToJson(value);
        }

        internal static Entity CloneEntity(Entity source, WorldInstance world)
        {
            ValidateEntityForRead(source, world);
            
            var newEntity = EntitiesManager.Allocate(world);
            
            var componentInfos = GetAllComponentInfos(source, world);
            
            foreach (var info in componentInfos)
            {
                var table = GetTableByType(info.ComponentType, world);
                if (table == null)
                {
                    throw new InvalidOperationException($"Component table for type {info.ComponentType.Name} does not exist. This should not happen when cloning an entity with existing components.");
                }
                
                table.AddComponentForEntity(newEntity, info.Value);
            }
            
            return newEntity;
        }
    }
}
