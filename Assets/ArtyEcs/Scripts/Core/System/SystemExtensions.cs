namespace ArtyECS.Core
{
    public static class SystemExtensions
    {
        public static void AddToUpdate(this SystemHandler system, World world = null)
        {
            SystemsManager.AddToUpdate(system, world);
        }

        public static void AddToUpdate(this SystemHandler system, int order, World world = null)
        {
            SystemsManager.AddToUpdate(system, order, world);
        }

        public static void AddToFixedUpdate(this SystemHandler system, World world = null)
        {
            SystemsManager.AddToFixedUpdate(system, world);
        }

        public static void AddToFixedUpdate(this SystemHandler system, int order, World world = null)
        {
            SystemsManager.AddToFixedUpdate(system, order, world);
        }

        public static void ExecuteOnce(this SystemHandler system, World world = null)
        {
            SystemsManager.ExecuteOnce(system, world);
        }
    }
}

