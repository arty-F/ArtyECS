namespace ArtyECS.Core
{
    public class SystemHandler
    {
        public virtual void Execute(World world)
        {
        }

        public override string ToString()
        {
            return $"SystemHandler({GetType().Name})";
        }
    }
}

