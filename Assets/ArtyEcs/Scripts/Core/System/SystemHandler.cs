namespace ArtyECS.Core
{
    public class SystemHandler
    {
        public virtual void Execute(WorldInstance world)
        {
        }

        public override string ToString()
        {
            return $"SystemHandler({GetType().Name})";
        }
    }
}

