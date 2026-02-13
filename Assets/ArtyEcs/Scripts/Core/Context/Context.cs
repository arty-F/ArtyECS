namespace ArtyECS.Core
{
    public class Context
    {
        public Entity Entity { get; private set; }
        internal int TypeId { get; set; }
        internal bool IsUniq { get; set; }
        internal bool IsTag { get; set; }

        internal void SetEntity(Entity entity) => Entity = entity;

        internal void Clear()
        {
            Entity = null;
            IsUniq = false;
            IsTag = false;
        }
    }
}

