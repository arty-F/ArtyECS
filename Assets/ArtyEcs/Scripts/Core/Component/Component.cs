namespace ArtyECS.Core
{
    public class Component
    {
        public Entity Entity { get; private set; }
        internal int TypeId { get; set; }
        internal bool Uniq { get; set; }

        internal void SetEntity(Entity entity) => Entity = entity;

        internal void Clear()
        {
            Entity = null;
            Uniq = false;
        }
    }
}

