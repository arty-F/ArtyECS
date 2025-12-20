using System;

namespace ArtyECS.Core
{
    public struct ComponentInfo
    {
        public Type ComponentType { get; set; }
        public object Value { get; set; }
        public string JsonValue { get; set; }

        public ComponentInfo(Type componentType, object value, string jsonValue = null)
        {
            ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
            Value = value;
            JsonValue = jsonValue;
        }
    }
}

