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
            var speed = entity.GetComponent<Speed>();

            position.X += movementDirection.X * speed.Value * deltaTime;
            position.Y += movementDirection.Y * speed.Value * deltaTime;
            position.Z += movementDirection.Z * speed.Value * deltaTime;
        }
    }
}

