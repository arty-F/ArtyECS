using ArtyECS.Core;
using UnityEngine;

public class TransformSyncSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<Position>().Execute();
        foreach (var entity in entities)
        {
            if (entity.GameObject != null)
            {
                var position = entity.Get<Position>();
                entity.GameObject.transform.position = new Vector3(position.X, position.Y, position.Z);
            }
        }
    }
}

