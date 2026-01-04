using UnityEngine;
using ArtyECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;

public class SystemTests : TestBase
{
    [ContextMenu(nameof(Test_RegisterSystem_Update))]
    public void Test_RegisterSystem_Update()
    {
        string testName = "Test_RegisterSystem_Update";
        ExecuteTest(testName, () =>
        {
            var system = new IncrementCounterSystem();
            World.RegisterSystem(system, UpdateType.Update);

            var entity = World.CreateEntity();
            entity.AddComponent(new CounterComponent { Value = 0 });

            World.ExecuteSystems(UpdateType.Update);

            var counter = entity.GetComponent<CounterComponent>();
            AssertEquals(1, counter.Value, "Counter should be incremented by system");
        });
    }

    [ContextMenu(nameof(Test_RegisterSystem_FixedUpdate))]
    public void Test_RegisterSystem_FixedUpdate()
    {
        string testName = "Test_RegisterSystem_FixedUpdate";
        ExecuteTest(testName, () =>
        {
            var system = new IncrementCounterSystem();
            World.RegisterSystem(system, UpdateType.FixedUpdate);

            var entity = World.CreateEntity();
            entity.AddComponent(new CounterComponent { Value = 0 });

            World.ExecuteSystems(UpdateType.FixedUpdate);

            var counter = entity.GetComponent<CounterComponent>();
            AssertEquals(1, counter.Value, "Counter should be incremented by system");
        });
    }

    [ContextMenu(nameof(Test_System_ModifiesComponent))]
    public void Test_System_ModifiesComponent()
    {
        string testName = "Test_System_ModifiesComponent";
        ExecuteTest(testName, () =>
        {
            var system = new MovementSystem();
            World.RegisterSystem(system, UpdateType.Update);

            var entity = World.CreateEntity();
            entity.AddComponent(new Position { X = 0f, Y = 0f, Z = 0f });
            entity.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            World.ExecuteSystems(UpdateType.Update);

            var position = entity.GetComponent<Position>();
            AssertEquals(1f, position.X, "Position X should be updated");
            AssertEquals(2f, position.Y, "Position Y should be updated");
            AssertEquals(3f, position.Z, "Position Z should be updated");
        });
    }

    [ContextMenu(nameof(Test_System_ExecutesMultipleTimes))]
    public void Test_System_ExecutesMultipleTimes()
    {
        string testName = "Test_System_ExecutesMultipleTimes";
        ExecuteTest(testName, () =>
        {
            var system = new IncrementCounterSystem();
            World.RegisterSystem(system, UpdateType.Update);

            var entity = World.CreateEntity();
            entity.AddComponent(new CounterComponent { Value = 0 });

            World.ExecuteSystems(UpdateType.Update);
            World.ExecuteSystems(UpdateType.Update);
            World.ExecuteSystems(UpdateType.Update);

            var counter = entity.GetComponent<CounterComponent>();
            AssertEquals(3, counter.Value, "Counter should be incremented 3 times");
        });
    }

    [ContextMenu(nameof(Test_MultipleSystems_UpdateQueue))]
    public void Test_MultipleSystems_UpdateQueue()
    {
        string testName = "Test_MultipleSystems_UpdateQueue";
        ExecuteTest(testName, () =>
        {
            var system1 = new IncrementUpdateCounterSystem();
            var system2 = new IncrementUpdateCounterSystem();
            
            World.RegisterSystem(system1, UpdateType.Update);
            World.RegisterSystem(system2, UpdateType.Update);

            var entity = World.CreateEntity();
            entity.AddComponent(new UpdateCounter { Value = 0 });

            World.ExecuteSystems(UpdateType.Update);

            var counter = entity.GetComponent<UpdateCounter>();
            AssertEquals(2, counter.Value, "Counter should be incremented by both systems");
        });
    }

    [ContextMenu(nameof(Test_MultipleSystems_FixedUpdateQueue))]
    public void Test_MultipleSystems_FixedUpdateQueue()
    {
        string testName = "Test_MultipleSystems_FixedUpdateQueue";
        ExecuteTest(testName, () =>
        {
            var system1 = new IncrementFixedUpdateCounterSystem();
            var system2 = new IncrementFixedUpdateCounterSystem();
            
            World.RegisterSystem(system1, UpdateType.FixedUpdate);
            World.RegisterSystem(system2, UpdateType.FixedUpdate);

            var entity = World.CreateEntity();
            entity.AddComponent(new FixedUpdateCounter { Value = 0 });

            World.ExecuteSystems(UpdateType.FixedUpdate);

            var counter = entity.GetComponent<FixedUpdateCounter>();
            AssertEquals(2, counter.Value, "Counter should be incremented by both systems");
        });
    }

    [ContextMenu(nameof(Test_System_ExecutionOrder))]
    public void Test_System_ExecutionOrder()
    {
        string testName = "Test_System_ExecutionOrder";
        ExecuteTest(testName, () =>
        {
            ExecutionOrderTrackerSystem.GlobalExecutionOrder = 0;
            
            var system1 = new ExecutionOrderTrackerSystem();
            var system2 = new ExecutionOrderTrackerSystem();
            var system3 = new ExecutionOrderTrackerSystem();
            
            World.RegisterSystem(system1, UpdateType.Update);
            World.RegisterSystem(system2, UpdateType.Update);
            World.RegisterSystem(system3, UpdateType.Update);

            World.ExecuteSystems(UpdateType.Update);

            AssertEquals(0, system1.ExecutionOrder, "First system should execute first");
            AssertEquals(1, system2.ExecutionOrder, "Second system should execute second");
            AssertEquals(2, system3.ExecutionOrder, "Third system should execute third");
        });
    }

    [ContextMenu(nameof(Test_System_OnlyExecutesForRegisteredWorld))]
    public void Test_System_OnlyExecutesForRegisteredWorld()
    {
        string testName = "Test_System_OnlyExecutesForRegisteredWorld";
        ExecuteTest(testName, () =>
        {
            var world1 = World.GetOrCreate("World1");
            var world2 = World.GetOrCreate("World2");

            var system1 = new IncrementCounterSystem();
            var system2 = new IncrementCounterSystem();
            
            world1.RegisterSystem(system1, UpdateType.Update);
            world2.RegisterSystem(system2, UpdateType.Update);

            var entity1 = world1.CreateEntity();
            entity1.AddComponent(new CounterComponent { Value = 0 });

            var entity2 = world2.CreateEntity();
            entity2.AddComponent(new CounterComponent { Value = 0 });

            world1.ExecuteSystems(UpdateType.Update);
            world2.ExecuteSystems(UpdateType.Update);

            var counter1 = entity1.GetComponent<CounterComponent>();
            var counter2 = entity2.GetComponent<CounterComponent>();
            
            AssertEquals(1, counter1.Value, "Entity1 counter should be incremented");
            AssertEquals(1, counter2.Value, "Entity2 counter should be incremented");
        });
    }

    [ContextMenu(nameof(Test_System_ProcessesMultipleEntities))]
    public void Test_System_ProcessesMultipleEntities()
    {
        string testName = "Test_System_ProcessesMultipleEntities";
        ExecuteTest(testName, () =>
        {
            var system = new IncrementCounterSystem();
            World.RegisterSystem(system, UpdateType.Update);

            var entity1 = World.CreateEntity();
            entity1.AddComponent(new CounterComponent { Value = 0 });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new CounterComponent { Value = 5 });

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new CounterComponent { Value = 10 });

            World.ExecuteSystems(UpdateType.Update);

            var counter1 = entity1.GetComponent<CounterComponent>();
            var counter2 = entity2.GetComponent<CounterComponent>();
            var counter3 = entity3.GetComponent<CounterComponent>();
            
            AssertEquals(1, counter1.Value, "Entity1 counter should be incremented");
            AssertEquals(6, counter2.Value, "Entity2 counter should be incremented");
            AssertEquals(11, counter3.Value, "Entity3 counter should be incremented");
        });
    }

    [ContextMenu(nameof(Test_System_WithQueryFilter))]
    public void Test_System_WithQueryFilter()
    {
        string testName = "Test_System_WithQueryFilter";
        ExecuteTest(testName, () =>
        {
            var system = new DamageSystem();
            World.RegisterSystem(system, UpdateType.Update);

            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Health { Amount = 100f });
            entity1.AddComponent(new Damage { Value = 10f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Health { Amount = 50f });

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Health { Amount = 75f });
            entity3.AddComponent(new Damage { Value = 5f });

            World.ExecuteSystems(UpdateType.Update);

            var health1 = entity1.GetComponent<Health>();
            var health2 = entity2.GetComponent<Health>();
            var health3 = entity3.GetComponent<Health>();
            
            AssertEquals(90f, health1.Amount, "Entity1 health should be reduced by damage");
            AssertEquals(50f, health2.Amount, "Entity2 health should remain unchanged (no damage component)");
            AssertEquals(70f, health3.Amount, "Entity3 health should be reduced by damage");
            
            var entitiesWithDamage = World.Query().With<Damage>().Execute().ToList();
            AssertEquals(0, entitiesWithDamage.Count, "No entities should have Damage component after processing");
        });
    }

    [ContextMenu(nameof(Test_System_UpdateAndFixedUpdate_Separate))]
    public void Test_System_UpdateAndFixedUpdate_Separate()
    {
        string testName = "Test_System_UpdateAndFixedUpdate_Separate";
        ExecuteTest(testName, () =>
        {
            var updateSystem = new IncrementUpdateCounterSystem();
            var fixedUpdateSystem = new IncrementFixedUpdateCounterSystem();
            
            World.RegisterSystem(updateSystem, UpdateType.Update);
            World.RegisterSystem(fixedUpdateSystem, UpdateType.FixedUpdate);

            var entity1 = World.CreateEntity();
            entity1.AddComponent(new UpdateCounter { Value = 0 });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new FixedUpdateCounter { Value = 0 });

            World.ExecuteSystems(UpdateType.Update);
            World.ExecuteSystems(UpdateType.FixedUpdate);

            var updateCounter = entity1.GetComponent<UpdateCounter>();
            var fixedUpdateCounter = entity2.GetComponent<FixedUpdateCounter>();
            
            AssertEquals(1, updateCounter.Value, "UpdateCounter should be incremented");
            AssertEquals(1, fixedUpdateCounter.Value, "FixedUpdateCounter should be incremented");
        });
    }

    [ContextMenu(nameof(Test_System_StatefulSystem))]
    public void Test_System_StatefulSystem()
    {
        string testName = "Test_System_StatefulSystem";
        ExecuteTest(testName, () =>
        {
            var system = new StatefulSystem();
            system.FieldValue = 42;
            
            World.RegisterSystem(system, UpdateType.Update);

            var executed = false;
            var testSystem = new TestSystem(() => 
            {
                executed = true;
                AssertEquals(42, system.FieldValue, "StatefulSystem should maintain state");
            });
            
            World.RegisterSystem(testSystem, UpdateType.Update);

            World.ExecuteSystems(UpdateType.Update);

            Assert(executed, "TestSystem should have executed");
        });
    }

    [ContextMenu(nameof(Test_System_ComplexMovement))]
    public void Test_System_ComplexMovement()
    {
        string testName = "Test_System_ComplexMovement";
        ExecuteTest(testName, () =>
        {
            var system = new MovementSystem();
            World.RegisterSystem(system, UpdateType.Update);

            var entity = World.CreateEntity();
            entity.AddComponent(new Position { X = 0f, Y = 0f, Z = 0f });
            entity.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            World.ExecuteSystems(UpdateType.Update);
            World.ExecuteSystems(UpdateType.Update);
            World.ExecuteSystems(UpdateType.Update);

            var position = entity.GetComponent<Position>();
            AssertEquals(3f, position.X, "Position X should be updated 3 times");
            AssertEquals(6f, position.Y, "Position Y should be updated 3 times");
            AssertEquals(9f, position.Z, "Position Z should be updated 3 times");
        });
    }

    [ContextMenu(nameof(Test_System_EmptyQuery))]
    public void Test_System_EmptyQuery()
    {
        string testName = "Test_System_EmptyQuery";
        ExecuteTest(testName, () =>
        {
            var system = new IncrementCounterSystem();
            World.RegisterSystem(system, UpdateType.Update);

            World.ExecuteSystems(UpdateType.Update);

            var entities = World.Query().With<CounterComponent>().Execute().ToList();
            AssertEquals(0, entities.Count, "Should have no entities with CounterComponent");
        });
    }

    [ContextMenu(nameof(Test_System_WorldInstance_RegisterSystem))]
    public void Test_System_WorldInstance_RegisterSystem()
    {
        string testName = "Test_System_WorldInstance_RegisterSystem";
        ExecuteTest(testName, () =>
        {
            var world = World.GetOrCreate("TestWorld");
            var system = new IncrementCounterSystem();
            
            world.RegisterSystem(system, UpdateType.Update);

            var entity = world.CreateEntity();
            entity.AddComponent(new CounterComponent { Value = 0 });

            world.ExecuteSystems(UpdateType.Update);

            var counter = entity.GetComponent<CounterComponent>();
            AssertEquals(1, counter.Value, "Counter should be incremented");
        });
    }
}

