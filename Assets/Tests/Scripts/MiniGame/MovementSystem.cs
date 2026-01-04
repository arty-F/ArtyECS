using ArtyECS.Core;
using UnityEngine;

public class MovementSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<Position>().With<MovementDirection>().With<Speed>().Execute();
        foreach (var entity in entities)
        {
            var position = entity.GetComponent<Position>();
            var movementDirection = entity.GetComponent<MovementDirection>();
            var speed = entity.GetComponent<Speed>();

            float currentSpeed = speed.Value;

            float deltaTime = Time.deltaTime;
            position.X += movementDirection.X * currentSpeed * deltaTime;
            position.Y += movementDirection.Y * currentSpeed * deltaTime;
            position.Z += movementDirection.Z * currentSpeed * deltaTime;

            entity.RemoveComponent(position);
            entity.AddComponent(position);
        }
    }
}

