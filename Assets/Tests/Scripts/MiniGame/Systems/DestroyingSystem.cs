using ArtyECS.Core;
using System.Linq;

public class DestroyingSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entitesToDestroy = World
            .Query()
            .With<Destroying>()
            .Execute()
            .ToArray();
        foreach (var entity in entitesToDestroy)
        {
            World.DestroyEntity(entity);
        }
    }
}

