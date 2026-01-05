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
            var position = entity.GetComponent<Position>();
            var movementDirection = entity.GetComponent<MoveDirection>();
            var speed = entity.GetComponent<Speed>().Value;

            if (entity.HasComponent<SpeedBonus>())
            {
                speed += entity.GetComponent<SpeedBonus>().Value;
            }

            position.X += movementDirection.X * speed * deltaTime;
            position.Y += movementDirection.Y * speed * deltaTime;
            position.Z += movementDirection.Z * speed * deltaTime;
        }
    }
}

