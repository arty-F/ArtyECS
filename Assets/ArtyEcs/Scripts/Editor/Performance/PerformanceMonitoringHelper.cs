#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ArtyECS.Core;

namespace ArtyECS.Core
{
    internal static class PerformanceMonitoringHelper
    {
        internal static (Array componentsArray, Array entitiesArray, Dictionary<Entity, int> dictionary)? GetTableInternalData(IComponentTable table, Type componentType)
        {
            var method = typeof(PerformanceMonitoringHelper).GetMethod("GetTableInternalDataGeneric", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method == null)
                return null;

            var genericMethod = method.MakeGenericMethod(componentType);
            try
            {
                var result = genericMethod.Invoke(null, new object[] { table });
                return ((Array, Array, Dictionary<Entity, int>))result;
            }
            catch
            {
                return null;
            }
        }

        private static (Array componentsArray, Array entitiesArray, Dictionary<Entity, int> dictionary)? GetTableInternalDataGeneric<T>(IComponentTable table) where T : struct, IComponent
        {
            if (table is ComponentTable<T> componentTable)
            {
                var data = componentTable.GetInternalData();
                return data;
            }
            return null;
        }
    }
}
#endif

