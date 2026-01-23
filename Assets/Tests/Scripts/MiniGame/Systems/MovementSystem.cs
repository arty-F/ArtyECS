using ArtyECS.Core;
using UnityEngine;

public class MovementSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world
            .Query()
            .With<Position>()
            .With<MoveDirection>()
            .With<Speed>()
            .Execute();
        float deltaTime = Time.deltaTime;
        foreach (var entity in entities)
        {
            var position = entity.Get<Position>();
            var movementDirection = entity.Get<MoveDirection>();
            var speed = entity.Get<Speed>().Value;

            if (entity.Have<SpeedBonus>())
            {
                speed += entity.Get<SpeedBonus>().Value;
            }

            position.X += movementDirection.X * speed * deltaTime;
            position.Y += movementDirection.Y * speed * deltaTime;
            position.Z += movementDirection.Z * speed * deltaTime;
        }
    }
}

