using ArtyECS.Core;
using System;
using UnityEngine;

// Test systems for tests
public class TestSystem : SystemHandler
{
    private Action executeAction;

    public TestSystem(Action executeAction)
    {
        this.executeAction = executeAction;
    }

    public override void Execute(WorldInstance world)
    {
        executeAction?.Invoke();
    }
}

public class StatefulSystem : SystemHandler
{
    public int FieldValue;
}

public class IncrementCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<CounterComponent>().Execute();
        foreach (var entity in entities)
        {
            var counter = entity.GetComponent<CounterComponent>();
            counter.Value++;
        }
    }
}

public class IncrementUpdateCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<UpdateCounter>().Execute();
        foreach (var entity in entities)
        {
            var counter = entity.GetComponent<UpdateCounter>();
            counter.Value++;
        }
    }
}

public class IncrementFixedUpdateCounterSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<FixedUpdateCounter>().Execute();
        foreach (var entity in entities)
        {
            var counter = entity.GetComponent<FixedUpdateCounter>();
            counter.Value++;
        }
    }
}

public class MovementSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<Position>().With<Velocity>().Execute();
        foreach (var entity in entities)
        {
            var position = entity.GetComponent<Position>();
            var velocity = entity.GetComponent<Velocity>();

            position.X += velocity.X;
            position.Y += velocity.Y;
            position.Z += velocity.Z;
        }
    }
}

public class DamageSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        var entities = world.Query().With<Health>().With<Damage>().Execute();
        foreach (var entity in entities)
        {
            var health = entity.GetComponent<Health>();
            var damage = entity.GetComponent<Damage>();

            health.Amount -= damage.Value;

            entity.RemoveComponent(new Damage());
        }
    }
}

public class ExecutionOrderTrackerSystem : SystemHandler
{
    public int ExecutionOrder { get; set; }
    public static int GlobalExecutionOrder = 0;

    public override void Execute(WorldInstance world)
    {
        ExecutionOrder = GlobalExecutionOrder++;
    }
}