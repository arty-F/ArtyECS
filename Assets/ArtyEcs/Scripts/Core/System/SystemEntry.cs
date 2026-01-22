namespace ArtyECS.Core
{
    internal class SystemEntry
    {
        internal SystemHandler System;
        internal WorldInstance World;

        internal SystemEntry(SystemHandler system, WorldInstance world)
        {
            System = system;
            World = world;
        }
    }
}

