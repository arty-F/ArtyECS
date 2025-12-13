using UnityEngine;
using ArtyECS.Core;
using System;

/// <summary>
/// Test class for System Framework functionality (System-000 through System-006).
/// </summary>
public class SystemTests : TestBase
{
    // ========== System-000: System Base Class ==========
    
    [ContextMenu("Run Test: System Instantiation")]
    public void Test_System_001()
    {
        string testName = "Test_System_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create instance of SystemHandler class
            SystemHandler system = new SystemHandler();
            
            // 2. Verify that instance is not null
            AssertNotNull(system, "System should not be null");
        });
    }
    
    [ContextMenu("Run Test: System Execute Method")]
    public void Test_System_002()
    {
        string testName = "Test_System_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create SystemHandler instance
            SystemHandler system = new SystemHandler();
            
            // 2. Call Execute() method
            WorldInstance world = World.GetOrCreate();
            system.Execute(world); // Should not throw exception
            
            // Method completes successfully
            Assert(true, "Execute() should complete without exception");
        });
    }
    
    [ContextMenu("Run Test: System Execute Override")]
    public void Test_System_003()
    {
        string testName = "Test_System_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem : SystemHandler with overridden Execute()
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // 2. Call Execute()
            WorldInstance world = World.GetOrCreate();
            testSystem.Execute(world);
            
            // 4. Check flag
            // Overridden Execute() is called
            Assert(flag, "Flag should be true after Execute()");
        });
    }
    
    [ContextMenu("Run Test: System State Support")]
    public void Test_System_004()
    {
        string testName = "Test_System_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create SystemHandler with instance field
            StatefulSystem system = new StatefulSystem();
            
            // 2. Set field value
            system.FieldValue = 42;
            
            // 3. Verify field value
            // System maintains state
            AssertEquals(42, system.FieldValue, "FieldValue should be 42");
        });
    }
    
    [ContextMenu("Run Test: System ToString")]
    public void Test_System_005()
    {
        string testName = "Test_System_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem instance
            TestSystem testSystem = new TestSystem(() => { });
            
            // 2. Call ToString()
            string str = testSystem.ToString();
            
            // String contains system type name
            Assert(str.Contains("TestSystem"), "ToString should contain 'TestSystem'");
        });
    }
    
    // ========== System-002: SystemsManager - Update Queue Management ==========
    
    [ContextMenu("Run Test: AddToUpdate Without Order")]
    public void Test_Update_001()
    {
        string testName = "Test_Update_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create System1
            TestSystem system1 = new TestSystem(() => { });
            
            // 2. Call World.AddToUpdate(System1)
            World.AddToUpdate(system1);
            
            // 3. Create System2
            TestSystem system2 = new TestSystem(() => { });
            
            // 4. Call World.AddToUpdate(System2)
            World.AddToUpdate(system2);
            
            // 5. Check queue order by executing
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            
            TestSystem testSystem1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem testSystem2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            World.AddToUpdate(testSystem1);
            World.AddToUpdate(testSystem2);
            
            World.ExecuteUpdate();
            
            // Systems added to end of queue
            AssertEquals(0, system1Order, "System1 should execute first");
            AssertEquals(1, system2Order, "System2 should execute second");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate With Order (Insert at Beginning)")]
    public void Test_Update_002()
    {
        string testName = "Test_Update_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create System1, System2
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            int system3Order = -1;
            
            TestSystem system1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            // 2. Add System1 to Update queue
            World.AddToUpdate(system1);
            
            // 3. Add System2 to Update queue
            World.AddToUpdate(system2);
            
            // 4. Create System3
            TestSystem system3 = new TestSystem(() => { system3Order = executionOrder++; });
            
            // 5. Call World.AddToUpdate(System3, order: 0)
            World.AddToUpdate(system3, 0);
            
            // 6. Execute and check order
            World.ExecuteUpdate();
            
            // System3 inserted at beginning, others shifted
            AssertEquals(0, system3Order, "System3 should execute first");
            AssertEquals(1, system1Order, "System1 should execute second");
            AssertEquals(2, system2Order, "System2 should execute third");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate With Order (Insert at Middle)")]
    public void Test_Update_003()
    {
        string testName = "Test_Update_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create System1, System2, System3
            int executionOrder = 0;
            int[] orders = new int[4];
            
            TestSystem system1 = new TestSystem(() => { orders[0] = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { orders[1] = executionOrder++; });
            TestSystem system3 = new TestSystem(() => { orders[2] = executionOrder++; });
            
            // 2. Add all to Update queue
            World.AddToUpdate(system1);
            World.AddToUpdate(system2);
            World.AddToUpdate(system3);
            
            // 3. Create System4
            TestSystem system4 = new TestSystem(() => { orders[3] = executionOrder++; });
            
            // 4. Call World.AddToUpdate(System4, order: 1)
            World.AddToUpdate(system4, 1);
            
            // 5. Execute and check order
            World.ExecuteUpdate();
            
            // System4 inserted at index 1, others shifted
            AssertEquals(0, orders[0], "System1 should execute first");
            AssertEquals(1, orders[3], "System4 should execute second");
            AssertEquals(2, orders[1], "System2 should execute third");
            AssertEquals(3, orders[2], "System3 should execute fourth");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate Invalid Order Throws Exception")]
    public void Test_Update_004()
    {
        string testName = "Test_Update_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create System
            TestSystem system = new TestSystem(() => { });
            
            // 2. Attempt to call World.AddToUpdate(System, order: -1)
            bool exceptionThrown = false;
            try
            {
                World.AddToUpdate(system, -1);
            }
            catch (ArgumentOutOfRangeException)
            {
                exceptionThrown = true;
            }
            
            // ArgumentOutOfRangeException is thrown
            Assert(exceptionThrown, "ArgumentOutOfRangeException should be thrown for negative order");
            
            // 3. Attempt to call world.AddToUpdate(System, order: 10) (when queue is empty)
            exceptionThrown = false;
            WorldInstance emptyWorld = World.GetOrCreate("EmptyTest");
            try
            {
                emptyWorld.AddToUpdate(system, 10);
            }
            catch (ArgumentOutOfRangeException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ArgumentOutOfRangeException should be thrown for order > queue count");
        });
    }
    
    // ========== System-003: SystemsManager - FixedUpdate Queue Management ==========
    
    [ContextMenu("Run Test: AddToFixedUpdate Without Order")]
    public void Test_FixedUpdate_001()
    {
        string testName = "Test_FixedUpdate_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create System1
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            
            TestSystem system1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            // 2. Call World.AddToFixedUpdate(System1)
            World.AddToFixedUpdate(system1);
            
            // 3. Create System2
            // 4. Call World.AddToFixedUpdate(System2)
            World.AddToFixedUpdate(system2);
            
            // 5. Execute and check order
            World.ExecuteFixedUpdate();
            
            // Systems added to end of queue
            AssertEquals(0, system1Order, "System1 should execute first");
            AssertEquals(1, system2Order, "System2 should execute second");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate With Order (Insert at Beginning)")]
    public void Test_FixedUpdate_002()
    {
        string testName = "Test_FixedUpdate_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create System1, System2
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            int system3Order = -1;
            
            TestSystem system1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            // 2. Add System1 to FixedUpdate queue
            World.AddToFixedUpdate(system1);
            
            // 3. Add System2 to FixedUpdate queue
            World.AddToFixedUpdate(system2);
            
            // 4. Create System3
            TestSystem system3 = new TestSystem(() => { system3Order = executionOrder++; });
            
            // 5. Call World.AddToFixedUpdate(System3, order: 0)
            World.AddToFixedUpdate(system3, 0);
            
            // 6. Execute and check order
            World.ExecuteFixedUpdate();
            
            // System3 inserted at beginning, others shifted
            AssertEquals(0, system3Order, "System3 should execute first");
            AssertEquals(1, system1Order, "System1 should execute second");
            AssertEquals(2, system2Order, "System2 should execute third");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate Invalid Order Throws Exception")]
    public void Test_FixedUpdate_003()
    {
        string testName = "Test_FixedUpdate_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create System
            TestSystem system = new TestSystem(() => { });
            
            // 2. Attempt to call World.AddToFixedUpdate(System, order: -1)
            bool exceptionThrown = false;
            try
            {
                World.AddToFixedUpdate(system, -1);
            }
            catch (ArgumentOutOfRangeException)
            {
                exceptionThrown = true;
            }
            
            // ArgumentOutOfRangeException is thrown
            Assert(exceptionThrown, "ArgumentOutOfRangeException should be thrown for negative order");
        });
    }
    
    // ========== System-004: SystemsManager - Manual Execution ==========
    
    [ContextMenu("Run Test: ExecuteOnce Executes System")]
    public void Test_ExecuteOnce_001()
    {
        string testName = "Test_ExecuteOnce_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem with Execute() that sets flag
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // 2. Call World.ExecuteOnce(TestSystem)
            World.ExecuteOnce(testSystem);
            
            // 3. Check flag
            // System executed immediately
            Assert(flag, "Flag should be true after ExecuteOnce");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce Does Not Add to Queue")]
    public void Test_ExecuteOnce_002()
    {
        string testName = "Test_ExecuteOnce_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create System
            TestSystem system = new TestSystem(() => { });
            
            // 2. Call World.ExecuteOnce(System)
            World.ExecuteOnce(system);
            
            // 3. Execute queues
            int executionCount = 0;
            TestSystem counterSystem = new TestSystem(() => { executionCount++; });
            World.AddToUpdate(counterSystem);
            World.ExecuteUpdate();
            
            // System not in any queue (counter should only increment once from counterSystem)
            AssertEquals(1, executionCount, "Only counterSystem should execute, not the ExecuteOnce system");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce Multiple Times")]
    public void Test_ExecuteOnce_003()
    {
        string testName = "Test_ExecuteOnce_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem with counter
            int counter = 0;
            TestSystem testSystem = new TestSystem(() => { counter++; });
            
            // 2. Call World.ExecuteOnce(TestSystem) three times
            World.ExecuteOnce(testSystem);
            World.ExecuteOnce(testSystem);
            World.ExecuteOnce(testSystem);
            
            // 3. Check counter
            // System executed three times
            AssertEquals(3, counter, "Counter should be 3");
        });
    }
    
    // ========== System-005: SystemsManager - Queue Execution (Sync) ==========
    
    [ContextMenu("Run Test: ExecuteUpdate Empty Queue")]
    public void Test_ExecuteUpdate_001()
    {
        string testName = "Test_ExecuteUpdate_001";
        ExecuteTest(testName, () =>
        {
            // 1. Ensure Update queue is empty
            
            // 2. Call World.ExecuteUpdate()
            // No exception, completes successfully
            World.ExecuteUpdate();
            Assert(true, "ExecuteUpdate should complete without exception on empty queue");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate Single System")]
    public void Test_ExecuteUpdate_002()
    {
        string testName = "Test_ExecuteUpdate_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem with Execute() that sets flag
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // 2. Add TestSystem to Update queue
            World.AddToUpdate(testSystem);
            
            // 3. Call World.ExecuteUpdate()
            World.ExecuteUpdate();
            
            // 4. Check flag
            // System executed
            Assert(flag, "Flag should be true after ExecuteUpdate");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate Multiple Systems in Order")]
    public void Test_ExecuteUpdate_003()
    {
        string testName = "Test_ExecuteUpdate_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create three systems with execution order tracking
            int[] executionOrder = new int[3];
            int order = 0;
            
            TestSystem system1 = new TestSystem(() => { executionOrder[0] = order++; });
            TestSystem system2 = new TestSystem(() => { executionOrder[1] = order++; });
            TestSystem system3 = new TestSystem(() => { executionOrder[2] = order++; });
            
            // 2. Add all to Update queue
            World.AddToUpdate(system1);
            World.AddToUpdate(system2);
            World.AddToUpdate(system3);
            
            // 3. Call World.ExecuteUpdate()
            World.ExecuteUpdate();
            
            // 4. Check execution order
            // Systems executed in queue order (0, 1, 2)
            AssertEquals(0, executionOrder[0], "System1 should execute first");
            AssertEquals(1, executionOrder[1], "System2 should execute second");
            AssertEquals(2, executionOrder[2], "System3 should execute third");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate Continues After Exception")]
    public void Test_ExecuteUpdate_004()
    {
        string testName = "Test_ExecuteUpdate_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem1 (throws exception)
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { throw new Exception("Test exception"); });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 2. Add both to Update queue
            World.AddToUpdate(system1);
            World.AddToUpdate(system2);
            
            // 3. Call World.ExecuteUpdate()
            World.ExecuteUpdate();
            
            // 4. Check flag
            // TestSystem2 executed despite TestSystem1 exception
            Assert(system2Executed, "System2 should execute despite System1 exception");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Empty Queue")]
    public void Test_ExecuteFixedUpdate_001()
    {
        string testName = "Test_ExecuteFixedUpdate_001";
        ExecuteTest(testName, () =>
        {
            // 1. Ensure FixedUpdate queue is empty
            
            // 2. Call World.ExecuteFixedUpdate()
            // No exception, completes successfully
            World.ExecuteFixedUpdate();
            Assert(true, "ExecuteFixedUpdate should complete without exception on empty queue");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Single System")]
    public void Test_ExecuteFixedUpdate_002()
    {
        string testName = "Test_ExecuteFixedUpdate_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem with Execute() that sets flag
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // 2. Add TestSystem to FixedUpdate queue
            World.AddToFixedUpdate(testSystem);
            
            // 3. Call World.ExecuteFixedUpdate()
            World.ExecuteFixedUpdate();
            
            // 4. Check flag
            // System executed
            Assert(flag, "Flag should be true after ExecuteFixedUpdate");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Multiple Systems in Order")]
    public void Test_ExecuteFixedUpdate_003()
    {
        string testName = "Test_ExecuteFixedUpdate_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create three systems with execution order tracking
            int[] executionOrder = new int[3];
            int order = 0;
            
            TestSystem system1 = new TestSystem(() => { executionOrder[0] = order++; });
            TestSystem system2 = new TestSystem(() => { executionOrder[1] = order++; });
            TestSystem system3 = new TestSystem(() => { executionOrder[2] = order++; });
            
            // 2. Add all to FixedUpdate queue
            World.AddToFixedUpdate(system1);
            World.AddToFixedUpdate(system2);
            World.AddToFixedUpdate(system3);
            
            // 3. Call World.ExecuteFixedUpdate()
            World.ExecuteFixedUpdate();
            
            // 4. Check execution order
            // Systems executed in queue order (0, 1, 2)
            AssertEquals(0, executionOrder[0], "System1 should execute first");
            AssertEquals(1, executionOrder[1], "System2 should execute second");
            AssertEquals(2, executionOrder[2], "System3 should execute third");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Continues After Exception")]
    public void Test_ExecuteFixedUpdate_004()
    {
        string testName = "Test_ExecuteFixedUpdate_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestSystem1 (throws exception)
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { throw new Exception("Test exception"); });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 2. Add both to FixedUpdate queue
            World.AddToFixedUpdate(system1);
            World.AddToFixedUpdate(system2);
            
            // 3. Call World.ExecuteFixedUpdate()
            World.ExecuteFixedUpdate();
            
            // 4. Check flag
            // TestSystem2 executed despite TestSystem1 exception
            Assert(system2Executed, "System2 should execute despite System1 exception");
        });
    }
    
    // ========== API-010: System Removal Methods ==========
    
    [ContextMenu("Run Test: RemoveFromUpdate")]
    public void Test_RemoveFromUpdate_001()
    {
        string testName = "Test_RemoveFromUpdate_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create System
            bool executed = false;
            TestSystem system = new TestSystem(() => { executed = true; });
            
            // 2. Add to Update queue
            World.AddToUpdate(system);
            
            // 3. Remove from Update queue
            bool removed = World.RemoveFromUpdate(system);
            
            // 4. Execute Update queue
            World.ExecuteUpdate();
            
            // 5. Verify system was removed
            Assert(removed, "RemoveFromUpdate should return true");
            Assert(!executed, "System should not execute after removal");
        });
    }
    
    [ContextMenu("Run Test: RemoveFromFixedUpdate")]
    public void Test_RemoveFromFixedUpdate_001()
    {
        string testName = "Test_RemoveFromFixedUpdate_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create System
            bool executed = false;
            TestSystem system = new TestSystem(() => { executed = true; });
            
            // 2. Add to FixedUpdate queue
            World.AddToFixedUpdate(system);
            
            // 3. Remove from FixedUpdate queue
            bool removed = World.RemoveFromFixedUpdate(system);
            
            // 4. Execute FixedUpdate queue
            World.ExecuteFixedUpdate();
            
            // 5. Verify system was removed
            Assert(removed, "RemoveFromFixedUpdate should return true");
            Assert(!executed, "System should not execute after removal");
        });
    }
    
    // ========== Real System Tests ==========
    
    [ContextMenu("Run Test: IncrementSystem")]
    public void Test_IncrementSystem_001()
    {
        string testName = "Test_IncrementSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities with CounterComponent
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            
            World.AddComponent(entity1, new CounterComponent { Value = 5 });
            World.AddComponent(entity2, new CounterComponent { Value = 10 });
            
            // 2. Create and execute IncrementSystem
            IncrementSystem system = new IncrementSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify counters were incremented
            CounterComponent counter1 = World.GetComponent<CounterComponent>(entity1);
            CounterComponent counter2 = World.GetComponent<CounterComponent>(entity2);
            
            AssertEquals(6, counter1.Value, "Counter1 should be 6");
            AssertEquals(11, counter2.Value, "Counter2 should be 11");
        });
    }
    
    [ContextMenu("Run Test: MovementSystem")]
    public void Test_MovementSystem_001()
    {
        string testName = "Test_MovementSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with Position and Velocity
            Entity entity = World.CreateEntity();
            
            World.AddComponent(entity, new Position { X = 0f, Y = 0f, Z = 0f });
            World.AddComponent(entity, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Create and execute MovementSystem
            MovementSystem system = new MovementSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify position was updated
            Position position = World.GetComponent<Position>(entity);
            
            AssertEquals(1f, position.X, "Position.X should be 1");
            AssertEquals(2f, position.Y, "Position.Y should be 2");
            AssertEquals(3f, position.Z, "Position.Z should be 3");
        });
    }
    
    [ContextMenu("Run Test: MovementSystem Multiple Entities")]
    public void Test_MovementSystem_002()
    {
        string testName = "Test_MovementSystem_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple entities with Position and Velocity
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            
            World.AddComponent(entity1, new Position { X = 0f, Y = 0f, Z = 0f });
            World.AddComponent(entity1, new Velocity { X = 1f, Y = 1f, Z = 1f });
            
            World.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            World.AddComponent(entity2, new Velocity { X = -1f, Y = -2f, Z = -3f });
            
            // 2. Create and execute MovementSystem
            MovementSystem system = new MovementSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify positions were updated
            Position pos1 = World.GetComponent<Position>(entity1);
            Position pos2 = World.GetComponent<Position>(entity2);
            
            AssertEquals(1f, pos1.X, "Entity1 Position.X should be 1");
            AssertEquals(1f, pos1.Y, "Entity1 Position.Y should be 1");
            AssertEquals(1f, pos1.Z, "Entity1 Position.Z should be 1");
            
            AssertEquals(9f, pos2.X, "Entity2 Position.X should be 9");
            AssertEquals(18f, pos2.Y, "Entity2 Position.Y should be 18");
            AssertEquals(27f, pos2.Z, "Entity2 Position.Z should be 27");
        });
    }
    
    [ContextMenu("Run Test: HealthSystem")]
    public void Test_HealthSystem_001()
    {
        string testName = "Test_HealthSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities with Health
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            
            World.AddComponent(entity1, new Health { Amount = 100f });
            World.AddComponent(entity2, new Health { Amount = 50f });
            
            // 2. Create and execute HealthSystem
            HealthSystem system = new HealthSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify health was decreased
            Health health1 = World.GetComponent<Health>(entity1);
            Health health2 = World.GetComponent<Health>(entity2);
            
            AssertEquals(99f, health1.Amount, "Health1 should be 99");
            AssertEquals(49f, health2.Amount, "Health2 should be 49");
        });
    }
    
    [ContextMenu("Run Test: HealthSystem Multiple Executions")]
    public void Test_HealthSystem_002()
    {
        string testName = "Test_HealthSystem_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with Health
            Entity entity = World.CreateEntity();
            
            World.AddComponent(entity, new Health { Amount = 100f });
            
            // 2. Execute HealthSystem multiple times
            HealthSystem system = new HealthSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            system.Execute(world);
            system.Execute(world);
            
            // 3. Verify health was decreased correctly
            Health health = World.GetComponent<Health>(entity);
            
            AssertEquals(97f, health.Amount, "Health should be 97 after 3 executions");
        });
    }
    
    [ContextMenu("Run Test: PhysicsSystem")]
    public void Test_PhysicsSystem_001()
    {
        string testName = "Test_PhysicsSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with Position, Velocity, and Acceleration
            Entity entity = World.CreateEntity();
            
            World.AddComponent(entity, new Position { X = 0f, Y = 0f, Z = 0f });
            World.AddComponent(entity, new Velocity { X = 1f, Y = 1f, Z = 1f });
            World.AddComponent(entity, new Acceleration { X = 0.5f, Y = 0.5f, Z = 0.5f });
            
            // 2. Create and execute PhysicsSystem
            PhysicsSystem system = new PhysicsSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify velocity was updated by acceleration
            Velocity velocity = World.GetComponent<Velocity>(entity);
            AssertEquals(1.5f, velocity.X, "Velocity.X should be 1.5");
            AssertEquals(1.5f, velocity.Y, "Velocity.Y should be 1.5");
            AssertEquals(1.5f, velocity.Z, "Velocity.Z should be 1.5");
            
            // 4. Verify position was updated by velocity
            Position position = World.GetComponent<Position>(entity);
            AssertEquals(1.5f, position.X, "Position.X should be 1.5");
            AssertEquals(1.5f, position.Y, "Position.Y should be 1.5");
            AssertEquals(1.5f, position.Z, "Position.Z should be 1.5");
        });
    }
    
    [ContextMenu("Run Test: SetValueSystem")]
    public void Test_SetValueSystem_001()
    {
        string testName = "Test_SetValueSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities with CounterComponent
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            
            World.AddComponent(entity1, new CounterComponent { Value = 5 });
            World.AddComponent(entity2, new CounterComponent { Value = 10 });
            
            // 2. Create and execute SetValueSystem with value 42
            SetValueSystem system = new SetValueSystem(42);
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify counters were set to 42
            CounterComponent counter1 = World.GetComponent<CounterComponent>(entity1);
            CounterComponent counter2 = World.GetComponent<CounterComponent>(entity2);
            
            AssertEquals(42, counter1.Value, "Counter1 should be 42");
            AssertEquals(42, counter2.Value, "Counter2 should be 42");
        });
    }
    
    [ContextMenu("Run Test: UpdateCounterSystem")]
    public void Test_UpdateCounterSystem_001()
    {
        string testName = "Test_UpdateCounterSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities with UpdateCounter
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            
            World.AddComponent(entity1, new UpdateCounter { Value = 0 });
            World.AddComponent(entity2, new UpdateCounter { Value = 5 });
            
            // 2. Add UpdateCounterSystem to Update queue and execute
            UpdateCounterSystem system = new UpdateCounterSystem();
            World.AddToUpdate(system);
            World.ExecuteUpdate();
            
            // 3. Verify counters were incremented
            UpdateCounter counter1 = World.GetComponent<UpdateCounter>(entity1);
            UpdateCounter counter2 = World.GetComponent<UpdateCounter>(entity2);
            
            AssertEquals(1, counter1.Value, "UpdateCounter1 should be 1");
            AssertEquals(6, counter2.Value, "UpdateCounter2 should be 6");
        });
    }
    
    [ContextMenu("Run Test: FixedUpdateCounterSystem")]
    public void Test_FixedUpdateCounterSystem_001()
    {
        string testName = "Test_FixedUpdateCounterSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities with FixedUpdateCounter
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            
            World.AddComponent(entity1, new FixedUpdateCounter { Value = 0 });
            World.AddComponent(entity2, new FixedUpdateCounter { Value = 5 });
            
            // 2. Add FixedUpdateCounterSystem to FixedUpdate queue and execute
            FixedUpdateCounterSystem system = new FixedUpdateCounterSystem();
            World.AddToFixedUpdate(system);
            World.ExecuteFixedUpdate();
            
            // 3. Verify counters were incremented
            FixedUpdateCounter counter1 = World.GetComponent<FixedUpdateCounter>(entity1);
            FixedUpdateCounter counter2 = World.GetComponent<FixedUpdateCounter>(entity2);
            
            AssertEquals(1, counter1.Value, "FixedUpdateCounter1 should be 1");
            AssertEquals(6, counter2.Value, "FixedUpdateCounter2 should be 6");
        });
    }
    
    [ContextMenu("Run Test: CleanupSystem")]
    public void Test_CleanupSystem_001()
    {
        string testName = "Test_CleanupSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities with Health and Dead components
            Entity entity1 = World.CreateEntity(); // Health > 0, should keep Dead
            Entity entity2 = World.CreateEntity(); // Health <= 0, should remove Dead
            
            World.AddComponent(entity1, new Health { Amount = 50f });
            World.AddComponent(entity1, new Dead { });
            
            World.AddComponent(entity2, new Health { Amount = 0f });
            World.AddComponent(entity2, new Dead { });
            
            // 2. Create and execute CleanupSystem
            CleanupSystem system = new CleanupSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify Dead component was removed only from entity2
            Assert(entity1.Has<Dead>(), "Entity1 should still have Dead component");
            Assert(!entity2.Has<Dead>(), "Entity2 should not have Dead component");
        });
    }
    
    [ContextMenu("Run Test: SpawnSystem")]
    public void Test_SpawnSystem_001()
    {
        string testName = "Test_SpawnSystem_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with Spawner component
            Entity spawnerEntity = World.CreateEntity();
            
            World.AddComponent(spawnerEntity, new Spawner { SpawnCount = 3 });
            
            // 2. Create and execute SpawnSystem
            SpawnSystem system = new SpawnSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            
            // 3. Verify new entity was created with TestComponent
            var entities = World.GetEntitiesWith<TestComponent>();
            AssertEquals(1, entities.Length, "Should have 1 entity with TestComponent");
            
            TestComponent testComponent = World.GetComponent<TestComponent>(entities[0]);
            AssertEquals(42, testComponent.Value, "TestComponent.Value should be 42");
            
            // 4. Verify SpawnCount was decremented
            Spawner spawner = World.GetComponent<Spawner>(spawnerEntity);
            AssertEquals(2, spawner.SpawnCount, "SpawnCount should be 2");
        });
    }
    
    [ContextMenu("Run Test: SpawnSystem Multiple Executions")]
    public void Test_SpawnSystem_002()
    {
        string testName = "Test_SpawnSystem_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with Spawner component
            Entity spawnerEntity = World.CreateEntity();
            
            World.AddComponent(spawnerEntity, new Spawner { SpawnCount = 3 });
            
            // 2. Execute SpawnSystem multiple times
            SpawnSystem system = new SpawnSystem();
            WorldInstance world = World.GetOrCreate();
            system.Execute(world);
            system.Execute(world);
            system.Execute(world);
            
            // 3. Verify multiple entities were created
            var entities = World.GetEntitiesWith<TestComponent>();
            AssertEquals(3, entities.Length, "Should have 3 entities with TestComponent");
            
            // 4. Verify SpawnCount was decremented
            Spawner spawner = World.GetComponent<Spawner>(spawnerEntity);
            AssertEquals(0, spawner.SpawnCount, "SpawnCount should be 0");
        });
    }
    
    // ========== GetModifiableComponent Tests ==========
    
    [ContextMenu("Run Test: GetModifiableComponent Single Component")]
    public void Test_GetModifiableComponent_001()
    {
        string testName = "Test_GetModifiableComponent_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with CounterComponent
            Entity entity = World.CreateEntity();
            
            World.AddComponent(entity, new CounterComponent { Value = 10 });
            
            // 2. Get modifiable component and modify it
            ref CounterComponent counter = ref World.GetModifiableComponent<CounterComponent>(entity);
            counter.Value = 42;
            
            // 3. Verify modification was applied
            CounterComponent readCounter = World.GetComponent<CounterComponent>(entity);
            AssertEquals(42, readCounter.Value, "Counter should be 42");
        });
    }
    
    [ContextMenu("Run Test: GetModifiableComponent Multiple Modifications")]
    public void Test_GetModifiableComponent_002()
    {
        string testName = "Test_GetModifiableComponent_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with Position
            Entity entity = World.CreateEntity();
            
            World.AddComponent(entity, new Position { X = 0f, Y = 0f, Z = 0f });
            
            // 2. Get modifiable component and modify multiple fields
            ref Position position = ref World.GetModifiableComponent<Position>(entity);
            position.X = 10f;
            position.Y = 20f;
            position.Z = 30f;
            
            // 3. Verify all modifications were applied
            Position readPosition = World.GetComponent<Position>(entity);
            AssertEquals(10f, readPosition.X, "Position.X should be 10");
            AssertEquals(20f, readPosition.Y, "Position.Y should be 20");
            AssertEquals(30f, readPosition.Z, "Position.Z should be 30");
        });
    }
    
    [ContextMenu("Run Test: GetModifiableComponent Increment")]
    public void Test_GetModifiableComponent_003()
    {
        string testName = "Test_GetModifiableComponent_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with CounterComponent
            Entity entity = World.CreateEntity();
            
            World.AddComponent(entity, new CounterComponent { Value = 5 });
            
            // 2. Get modifiable component and increment it
            ref CounterComponent counter = ref World.GetModifiableComponent<CounterComponent>(entity);
            counter.Value++;
            counter.Value += 10;
            
            // 3. Verify modification was applied
            CounterComponent readCounter = World.GetComponent<CounterComponent>(entity);
            AssertEquals(16, readCounter.Value, "Counter should be 16");
        });
    }
    
    [ContextMenu("Run Test: GetModifiableComponent Multiple Entities")]
    public void Test_GetModifiableComponent_004()
    {
        string testName = "Test_GetModifiableComponent_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple entities with CounterComponent
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            World.AddComponent(entity1, new CounterComponent { Value = 1 });
            World.AddComponent(entity2, new CounterComponent { Value = 2 });
            World.AddComponent(entity3, new CounterComponent { Value = 3 });
            
            // 2. Modify each entity's component
            ref CounterComponent counter1 = ref World.GetModifiableComponent<CounterComponent>(entity1);
            ref CounterComponent counter2 = ref World.GetModifiableComponent<CounterComponent>(entity2);
            ref CounterComponent counter3 = ref World.GetModifiableComponent<CounterComponent>(entity3);
            
            counter1.Value = 10;
            counter2.Value = 20;
            counter3.Value = 30;
            
            // 3. Verify all modifications were applied correctly
            AssertEquals(10, World.GetComponent<CounterComponent>(entity1).Value, "Counter1 should be 10");
            AssertEquals(20, World.GetComponent<CounterComponent>(entity2).Value, "Counter2 should be 20");
            AssertEquals(30, World.GetComponent<CounterComponent>(entity3).Value, "Counter3 should be 30");
        });
    }
    
    [ContextMenu("Run Test: GetModifiableComponent Throws When Component Missing")]
    public void Test_GetModifiableComponent_005()
    {
        string testName = "Test_GetModifiableComponent_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity without CounterComponent
            Entity entity = World.CreateEntity();
            
            // 2. Attempt to get modifiable component
            bool exceptionThrown = false;
            try
            {
                ref CounterComponent counter = ref World.GetModifiableComponent<CounterComponent>(entity);
                counter.Value = 42; // This line should not be reached
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                exceptionThrown = true;
            }
            
            // 3. Verify exception was thrown
            Assert(exceptionThrown, "KeyNotFoundException should be thrown when component is missing");
        });
    }
    
    [ContextMenu("Run Test: GetModifiableComponent Physics Example")]
    public void Test_GetModifiableComponent_006()
    {
        string testName = "Test_GetModifiableComponent_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity with Position and Velocity (like PhysicsSystem does)
            Entity entity = World.CreateEntity();
            
            World.AddComponent(entity, new Position { X = 0f, Y = 0f, Z = 0f });
            World.AddComponent(entity, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Update position using velocity (simulating physics)
            Velocity velocity = World.GetComponent<Velocity>(entity);
            ref Position position = ref World.GetModifiableComponent<Position>(entity);
            
            position.X += velocity.X;
            position.Y += velocity.Y;
            position.Z += velocity.Z;
            
            // 3. Verify position was updated
            Position readPosition = World.GetComponent<Position>(entity);
            AssertEquals(1f, readPosition.X, "Position.X should be 1");
            AssertEquals(2f, readPosition.Y, "Position.Y should be 2");
            AssertEquals(3f, readPosition.Z, "Position.Z should be 3");
        });
    }
}

