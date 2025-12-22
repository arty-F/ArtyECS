#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    public static class ComponentQueueAnalyzer
    {
        private static Dictionary<(WorldInstance, Type, SystemHandler), bool> _usageCache =
            new Dictionary<(WorldInstance, Type, SystemHandler), bool>();

        private static readonly HashSet<string> ComponentQueryMethodNames = new HashSet<string>
        {
            nameof(WorldInstance.GetComponents),
            nameof(WorldInstance.GetEntitiesWith),
            nameof(WorldInstance.GetEntitiesWithout),
            nameof(WorldInstance.GetModifiableComponents),
            nameof(WorldInstance.GetComponent),
            nameof(WorldInstance.GetModifiableComponent),
            nameof(WorldInstance.Query)
        };

        private static readonly HashSet<string> QueryBuilderMethodNames = new HashSet<string>
        {
            nameof(QueryBuilder.With),
            nameof(QueryBuilder.Without)
        };

        public static Dictionary<string, List<SystemHandler>> GetComponentSystems(
            WorldInstance world,
            Type componentType)
        {
            var result = new Dictionary<string, List<SystemHandler>>();
            
            if (world == null || componentType == null)
            {
                return result;
            }

            var updateQueue = world.GetUpdateQueue();
            var fixedUpdateQueue = world.GetFixedUpdateQueue();

            var updateSystems = new List<SystemHandler>();
            var fixedUpdateSystems = new List<SystemHandler>();

            foreach (var system in updateQueue)
            {
                if (DoesSystemUseComponent(system, componentType, world))
                {
                    updateSystems.Add(system);
                }
            }

            foreach (var system in fixedUpdateQueue)
            {
                if (DoesSystemUseComponent(system, componentType, world))
                {
                    fixedUpdateSystems.Add(system);
                }
            }

            if (updateSystems.Count > 0)
            {
                result["Update"] = updateSystems;
            }

            if (fixedUpdateSystems.Count > 0)
            {
                result["FixedUpdate"] = fixedUpdateSystems;
            }

            return result;
        }

        public static HashSet<string> GetComponentQueues(WorldInstance world, Type componentType)
        {
            var systems = GetComponentSystems(world, componentType);
            return new HashSet<string>(systems.Keys);
        }

        private static bool DoesSystemUseComponent(
            SystemHandler system,
            Type componentType,
            WorldInstance world)
        {
            var cacheKey = (world, componentType, system);
            
            if (_usageCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            bool result = AnalyzeSystemForComponent(system, componentType);
            _usageCache[cacheKey] = result;
            
            return result;
        }

        private static bool AnalyzeSystemForComponent(SystemHandler system, Type componentType)
        {
            Type systemType = system.GetType();
            MethodInfo executeMethod = systemType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);

            if (executeMethod == null)
            {
                return false;
            }

            try
            {
                var componentTypes = ExtractComponentTypesFromMethod(executeMethod);
                return componentTypes.Contains(componentType);
            }
            catch
            {
                return false;
            }
        }

        private static HashSet<Type> ExtractComponentTypesFromMethod(MethodInfo method)
        {
            var componentTypes = new HashSet<Type>();

            try
            {
                var methodBody = method.GetMethodBody();
                if (methodBody == null)
                {
                    return componentTypes;
                }

                var ilBytes = methodBody.GetILAsByteArray();
                if (ilBytes == null || ilBytes.Length == 0)
                {
                    return componentTypes;
                }

                ParseILForComponentTypes(method, ilBytes, componentTypes);
            }
            catch
            {
            }

            return componentTypes;
        }

        private static void ParseILForComponentTypes(
            MethodInfo method,
            byte[] ilBytes,
            HashSet<Type> componentTypes)
        {
            var opCodeMap = BuildOpCodeMap();
            int offset = 0;

            while (offset < ilBytes.Length)
            {
                ushort opCodeValue = ilBytes[offset];
                OpCode opCode;

                if (opCodeValue == 0xFE)
                {
                    if (offset + 1 >= ilBytes.Length)
                        break;
                    opCodeValue = (ushort)((ilBytes[offset] << 8) | ilBytes[offset + 1]);
                    offset += 2;
                }
                else
                {
                    offset += 1;
                }

                if (!opCodeMap.TryGetValue(opCodeValue, out opCode))
                {
                    break;
                }

                if (opCode.OperandType == OperandType.InlineMethod || opCode.OperandType == OperandType.InlineTok)
                {
                    if (offset + 4 > ilBytes.Length)
                        break;

                    int token = BitConverter.ToInt32(ilBytes, offset);
                    offset += 4;

                    try
                    {
                        MemberInfo member = method.Module.ResolveMember(token);
                        if (member is MethodInfo methodInfo)
                        {
                            ExtractComponentTypesFromMethodCall(methodInfo, componentTypes);
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    offset += GetOperandSize(opCode.OperandType, ilBytes, offset);
                }
            }
        }

        private static Dictionary<ushort, OpCode> BuildOpCodeMap()
        {
            var map = new Dictionary<ushort, OpCode>();
            var fields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
            
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(OpCode))
                {
                    var opCode = (OpCode)field.GetValue(null);
                    map[(ushort)opCode.Value] = opCode;
                }
            }
            
            return map;
        }

        private static int GetOperandSize(OperandType operandType, byte[] ilBytes, int offset)
        {
            switch (operandType)
            {
                case OperandType.InlineNone:
                    return 0;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    return 1;
                case OperandType.InlineBrTarget:
                case OperandType.InlineI:
                case OperandType.InlineVar:
                case OperandType.InlineString:
                    return 4;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    return 8;
                case OperandType.ShortInlineR:
                    return 4;
                case OperandType.InlineSwitch:
                    if (offset + 4 > ilBytes.Length)
                        return 0;
                    int count = BitConverter.ToInt32(ilBytes, offset);
                    return 4 + (count * 4);
                default:
                    return 0;
            }
        }

        private static void ExtractComponentTypesFromMethodCall(
            MethodInfo methodInfo,
            HashSet<Type> componentTypes)
        {
            if (methodInfo == null)
            {
                return;
            }

            string methodName = methodInfo.Name;
            Type declaringType = methodInfo.DeclaringType;

            if (declaringType == typeof(WorldInstance) && ComponentQueryMethodNames.Contains(methodName))
            {
                if (methodName == nameof(WorldInstance.Query))
                {
                    return;
                }

                if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
                {
                    var genericArgs = methodInfo.GetGenericArguments();
                    foreach (var arg in genericArgs)
                    {
                        if (typeof(IComponent).IsAssignableFrom(arg) && arg.IsValueType)
                        {
                            componentTypes.Add(arg);
                        }
                    }
                }
            }
            else if (declaringType == typeof(QueryBuilder) && QueryBuilderMethodNames.Contains(methodName))
            {
                if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
                {
                    var genericArgs = methodInfo.GetGenericArguments();
                    foreach (var arg in genericArgs)
                    {
                        if (typeof(IComponent).IsAssignableFrom(arg) && arg.IsValueType)
                        {
                            componentTypes.Add(arg);
                        }
                    }
                }
            }
        }

        public static void ClearCache()
        {
            _usageCache.Clear();
        }

        public static void ClearCacheForWorld(WorldInstance world)
        {
            var keysToRemove = _usageCache.Keys
                .Where(k => k.Item1 == world)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _usageCache.Remove(key);
            }
        }
    }
}
#endif

