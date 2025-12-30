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
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetComponent, world, typeof(T).Name))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetComponent<{typeof(T).Name}>", world))
#endif
            {
                ValidateEntityForRead(entity, world);

                var table = GetOrCreateTable<T>(world);

                if (table.TryGetComponent(entity, out T component))
                {
                    return component;
                }

                throw new ComponentNotFoundException(entity, typeof(T));
            }
        }

        internal static bool HasComponent<T>(Entity entity, WorldInstance world) where T : struct, IComponent
        {
            ValidateEntityForRead(entity, world);

            var table = GetOrCreateTable<T>(world);
            return table.HasComponent(entity);
        }

        internal static ref T GetModifiableComponent<T>(Entity entity, WorldInstance world) where T : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetModifiableComponent, world, typeof(T).Name))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetModifiableComponent<{typeof(T).Name}>", world))
#endif
            {
                ValidateEntityForRead(entity, world);

                var table = GetOrCreateTable<T>(world);
                return ref table.GetModifiableComponentRef(entity);
            }
        }

        internal static ReadOnlySpan<T> GetComponents<T>(WorldInstance world) where T : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetComponents, world, typeof(T).Name))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetComponents<{typeof(T).Name}>", world))
#endif
            {
                var table = GetOrCreateTable<T>(world);
                return table.GetComponents();
            }
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1>(WorldInstance world) where T1 : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetEntitiesWith1, world, typeof(T1).Name))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetEntitiesWith<{typeof(T1).Name}>", world))
#endif
            {
                var table = GetOrCreateTable<T1>(world);
                return table.GetEntities();
            }
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetEntitiesWith2, world, $"{typeof(T1).Name}, {typeof(T2).Name}"))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetEntitiesWith<{typeof(T1).Name},{typeof(T2).Name}>", world))
#endif
            {
                var table1 = GetOrCreateTable<T1>(world);
                var table2 = GetOrCreateTable<T2>(world);

                if (table1.Count == 0 || table2.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                using var intersection = table1.Count <= table2.Count 
                    ? table1.GetEntitiesSetPooled() 
                    : table2.GetEntitiesSetPooled();
                using var set2 = table1.Count <= table2.Count 
                    ? table2.GetEntitiesSetPooled() 
                    : table1.GetEntitiesSetPooled();
                ReadOnlySpan<Entity> baseEntities = table1.Count <= table2.Count 
                    ? table1.GetEntities() 
                    : table2.GetEntities();
                
                intersection.Set.IntersectWith(set2.Set);

                if (intersection.Set.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                var resultArray = QueryContext.Get(world).RentArray(intersection.Set.Count);
                int index = 0;
                foreach (var entity in baseEntities)
                {
                    if (intersection.Set.Contains(entity))
                    {
                        resultArray[index++] = entity;
                    }
                }

                return new ReadOnlySpan<Entity>(resultArray, 0, intersection.Set.Count);
            }
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetEntitiesWith3, world, $"{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}"))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetEntitiesWith<{typeof(T1).Name},{typeof(T2).Name},{typeof(T3).Name}>", world))
#endif
            {
                var table1 = GetOrCreateTable<T1>(world);
                var table2 = GetOrCreateTable<T2>(world);
                var table3 = GetOrCreateTable<T3>(world);

                if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                int minCount = Math.Min(Math.Min(table1.Count, table2.Count), table3.Count);
                PooledHashSet<Entity> intersection;
                ReadOnlySpan<Entity> baseEntities;

                if (table1.Count == minCount)
                {
                    intersection = table1.GetEntitiesSetPooled();
                    baseEntities = table1.GetEntities();
                    using var set2 = table2.GetEntitiesSetPooled();
                    using var set3 = table3.GetEntitiesSetPooled();
                    intersection.Set.IntersectWith(set2.Set);
                    intersection.Set.IntersectWith(set3.Set);
                }
                else if (table2.Count == minCount)
                {
                    intersection = table2.GetEntitiesSetPooled();
                    baseEntities = table2.GetEntities();
                    using var set1 = table1.GetEntitiesSetPooled();
                    using var set3 = table3.GetEntitiesSetPooled();
                    intersection.Set.IntersectWith(set1.Set);
                    intersection.Set.IntersectWith(set3.Set);
                }
                else
                {
                    intersection = table3.GetEntitiesSetPooled();
                    baseEntities = table3.GetEntities();
                    using var set1 = table1.GetEntitiesSetPooled();
                    using var set2 = table2.GetEntitiesSetPooled();
                    intersection.Set.IntersectWith(set1.Set);
                    intersection.Set.IntersectWith(set2.Set);
                }

                if (intersection.Set.Count == 0)
                {
                    intersection.Dispose();
                    return ReadOnlySpan<Entity>.Empty;
                }

                int resultCount = intersection.Set.Count;
                var resultArray = QueryContext.Get(world).RentArray(resultCount);
                int index = 0;
                foreach (var entity in baseEntities)
                {
                    if (intersection.Set.Contains(entity))
                    {
                        resultArray[index++] = entity;
                    }
                }

                intersection.Dispose();
                return new ReadOnlySpan<Entity>(resultArray, 0, resultCount);
            }
        }

        internal static HashSet<Entity> GetAllEntitiesInWorld(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            var allEntities = QueryContext.Get(world).RentHashSet();

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
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetEntitiesWithout1, world, $"!{typeof(T1).Name}"))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetEntitiesWithout<{typeof(T1).Name}>", world))
#endif
            {
                var allEntities = GetAllEntitiesInWorld(world);
                
                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                var table1 = GetOrCreateTable<T1>(world);
                using var exclusionSet = table1.GetEntitiesSetPooled();
                
                allEntities.ExceptWith(exclusionSet.Set);

                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                var resultArray = QueryContext.Get(world).RentArray(allEntities.Count);
                int index = 0;
                foreach (var entity in allEntities)
                {
                    resultArray[index++] = entity;
                }

                return new ReadOnlySpan<Entity>(resultArray, 0, allEntities.Count);
            }
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetEntitiesWithout2, world, $"!{typeof(T1).Name}, !{typeof(T2).Name}"))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetEntitiesWithout<{typeof(T1).Name},{typeof(T2).Name}>", world))
#endif
            {
                var allEntities = GetAllEntitiesInWorld(world);
                
                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                var table1 = GetOrCreateTable<T1>(world);
                var table2 = GetOrCreateTable<T2>(world);
                
                using var exclusionSet = EntityHashSetPool.RentPooled(table1.Count + table2.Count);
                
                var entities1 = table1.GetEntities();
                foreach (var entity in entities1)
                {
                    exclusionSet.Set.Add(entity);
                }
                
                var entities2 = table2.GetEntities();
                foreach (var entity in entities2)
                {
                    exclusionSet.Set.Add(entity);
                }
                
                allEntities.ExceptWith(exclusionSet.Set);

                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                var resultArray = QueryContext.Get(world).RentArray(allEntities.Count);
                int index = 0;
                foreach (var entity in allEntities)
                {
                    resultArray[index++] = entity;
                }

                return new ReadOnlySpan<Entity>(resultArray, 0, allEntities.Count);
            }
        }

        internal static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetEntitiesWithout3, world, $"!{typeof(T1).Name}, !{typeof(T2).Name}, !{typeof(T3).Name}"))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetEntitiesWithout<{typeof(T1).Name},{typeof(T2).Name},{typeof(T3).Name}>", world))
#endif
            {
                var allEntities = GetAllEntitiesInWorld(world);
                
                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                var table1 = GetOrCreateTable<T1>(world);
                var table2 = GetOrCreateTable<T2>(world);
                var table3 = GetOrCreateTable<T3>(world);
                
                using var exclusionSet = EntityHashSetPool.RentPooled(table1.Count + table2.Count + table3.Count);
                
                var entities1 = table1.GetEntities();
                foreach (var entity in entities1)
                {
                    exclusionSet.Set.Add(entity);
                }
                
                var entities2 = table2.GetEntities();
                foreach (var entity in entities2)
                {
                    exclusionSet.Set.Add(entity);
                }
                
                var entities3 = table3.GetEntities();
                foreach (var entity in entities3)
                {
                    exclusionSet.Set.Add(entity);
                }
                
                allEntities.ExceptWith(exclusionSet.Set);

                if (allEntities.Count == 0)
                {
                    return ReadOnlySpan<Entity>.Empty;
                }

                var resultArray = QueryContext.Get(world).RentArray(allEntities.Count);
                int index = 0;
                foreach (var entity in allEntities)
                {
                    resultArray[index++] = entity;
                }

                return new ReadOnlySpan<Entity>(resultArray, 0, allEntities.Count);
            }
        }

        internal static ModifiableComponentCollection<T> GetModifiableComponents<T>(WorldInstance world) where T : struct, IComponent
        {
#if UNITY_EDITOR
            using (PerformanceMonitoring.StartQueryTiming(QueryType.GetModifiableComponents, world, typeof(T).Name))
            using (PerformanceMonitoring.StartAllocationTracking($"Query:GetModifiableComponents<{typeof(T).Name}>", world))
#endif
            {
                var table = GetOrCreateTable<T>(world);
                return new ModifiableComponentCollection<T>(table);
            }
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
#if UNITY_EDITOR
            PerformanceMonitoring.ClearQueryTimings(world);
#endif
        }

        internal static void ClearAll()
        {
            TableCache.Clear();
            WorldTables.Clear();
#if UNITY_EDITOR
            PerformanceMonitoring.ClearAll();
#endif
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

#if UNITY_EDITOR
        internal static QueryTimingData? GetQueryTiming(QueryType queryType, WorldInstance world)
        {
            return PerformanceMonitoring.GetQueryTiming(queryType, world);
        }

        internal static List<QueryTimingData> GetAllQueryTimings(WorldInstance world)
        {
            return PerformanceMonitoring.GetAllQueryTimings(world);
        }

        internal static void ResetQueryTimings(WorldInstance world)
        {
            PerformanceMonitoring.ResetQueryTimings(world);
        }
#endif

#if UNITY_EDITOR
        internal static Dictionary<Type, IComponentTable> GetWorldTablesForMonitoring(WorldInstance world)
        {
            if (world == null || !WorldTables.TryGetValue(world, out var worldTable))
                return null;
            return worldTable;
        }

        internal static Dictionary<WorldInstance, Dictionary<Type, IComponentTable>> GetAllWorldTablesForMonitoring()
        {
            return WorldTables;
        }

        internal static Dictionary<(WorldInstance world, Type type), IComponentTable> GetTableCacheForMonitoring()
        {
            return TableCache;
        }

        internal static (Type componentType, Array componentsArray, Array entitiesArray, int dictionaryCount)? GetTableDataForMonitoring(IComponentTable table)
        {
            if (table == null)
                return null;

            foreach (var worldTable in WorldTables.Values)
            {
                foreach (var kvp in worldTable)
                {
                    if (ReferenceEquals(kvp.Value, table))
                    {
                        var componentType = kvp.Key;
                        var internalData = GetTableInternalDataForType(table, componentType);
                        if (internalData.HasValue)
                        {
                            var (componentsArray, entitiesArray, dictionary) = internalData.Value;
                            return (componentType, componentsArray, entitiesArray, dictionary.Count);
                        }
                    }
                }
            }
            return null;
        }

        private static (Array componentsArray, Array entitiesArray, Dictionary<Entity, int> dictionary)? GetTableInternalDataForType(IComponentTable table, Type componentType)
        {
            return PerformanceMonitoringHelper.GetTableInternalData(table, componentType);
        }
#endif
    }
}
