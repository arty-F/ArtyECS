namespace ArtyECS.Core
{
    internal class ComponentWrapper
    {
        internal int Id { get; private set; }
        internal IComponent Component { get; private set; }

        public ComponentWrapper(int id, IComponent component)
        {
            Id = id;
            Component = component;
        }
    }
}
