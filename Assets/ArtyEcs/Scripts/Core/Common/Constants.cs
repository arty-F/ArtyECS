namespace ArtyECS.Core
{
    internal static class Constants
    {
        internal static int ENTITY_POOL_CAPACITY = 64;
        internal static int ENTITY_COMPONENTS_CAPACITY = 16;
        internal static int COMPONENT_TYPES_POOL_CAPACITY = 32;
        internal static int COMPONENT_POOL_CAPACITY = 16;
        internal static int WORLD_ENTITIES_CAPACITY = 64;
        internal static int WORLD_ARCHETYPES_CAPACITY = 32;
        internal static int DEFAULT_ARCHETYPE_CAPACITY = 16;
        internal static int SYSTEMS_CAPACITY = 32;
        internal static int QUERY_BUILDERS_CAPACITY = 8;
        internal const string GAMEOBJECT_NAME = "ArtyEcsUpdateProvider";
        internal const int DEFAULT_ORDER = 100;
    }
}
