using ArtyECS.Core;

public class DestroyingSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entitesToDestroy = World
            .Query()
            .With<Destroying>()
            .Execute();
        foreach (var entity in entitesToDestroy)
        {
            World.DestroyEntity(entity);
        }
    }
}

