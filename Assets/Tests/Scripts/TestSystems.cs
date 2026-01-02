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

public class IncrementSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        //var counters = world.GetModifiableComponents<CounterComponent>();
        /*for (int i = 0; i < counters.Count; i++)
        {
            counters[i].Value++;
        }*/
    }
}

public class CleanEntitiesWithout3QueryBuilderSystem : SystemHandler
{
    public override void Execute(WorldInstance world)
    {
        /*var cleanEntities = world.Query()
            .Without<Dead>()
            .Without<Destroyed>()
            .Without<ExpirationTime>()
            .Execute();*/
        
        /*foreach (var entity in cleanEntities)
        {
            if (entity.Has<Enemy>(world))
            {
            }
        }*/
    }
}