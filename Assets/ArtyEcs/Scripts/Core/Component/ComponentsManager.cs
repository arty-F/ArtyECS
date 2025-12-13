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

            var currentCount = table.Count;
            var (components, entities, entityToIndex) = table.GetInternalTableForAdd(currentCount + 1);
            ref int count = ref table.GetCountRef();

            components[count] = component;
            entities[count] = entity;

            entityToIndex[entity] = count;

            count++;
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

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);
            var table4 = GetOrCreateTable<T4>(world);

            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0 || table4.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            int minCount = Math.Min(Math.Min(table1.Count, table2.Count), Math.Min(table3.Count, table4.Count));
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
            }
            else if (table3.Count == minCount)
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
            }
            else
            {
                intersection = table4.GetEntitiesSet();
                baseEntities = table4.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
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

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);
            var table4 = GetOrCreateTable<T4>(world);
            var table5 = GetOrCreateTable<T5>(world);

            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0 || table4.Count == 0 || table5.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            int minCount = Math.Min(Math.Min(Math.Min(table1.Count, table2.Count), Math.Min(table3.Count, table4.Count)), table5.Count);
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else if (table3.Count == minCount)
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else if (table4.Count == minCount)
            {
                intersection = table4.GetEntitiesSet();
                baseEntities = table4.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else
            {
                intersection = table5.GetEntitiesSet();
                baseEntities = table5.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
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

        internal static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5, T6>(WorldInstance world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);
            var table4 = GetOrCreateTable<T4>(world);
            var table5 = GetOrCreateTable<T5>(world);
            var table6 = GetOrCreateTable<T6>(world);

            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0 || table4.Count == 0 || table5.Count == 0 || table6.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            int minCount = Math.Min(Math.Min(Math.Min(table1.Count, table2.Count), Math.Min(table3.Count, table4.Count)), 
                Math.Min(table5.Count, table6.Count));
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table3.Count == minCount)
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table4.Count == minCount)
            {
                intersection = table4.GetEntitiesSet();
                baseEntities = table4.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table5.Count == minCount)
            {
                intersection = table5.GetEntitiesSet();
                baseEntities = table5.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else
            {
                intersection = table6.GetEntitiesSet();
                baseEntities = table6.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
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

        internal static ModifiableComponentCollection<T> GetModifiableComponents<T>(WorldInstance world) where T : struct, IComponent
        {
            var table = GetOrCreateTable<T>(world);
            return new ModifiableComponentCollection<T>(table, world);
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
    }
}
