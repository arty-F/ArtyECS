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
            World world = World.GetOrCreate();
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
            World world = World.GetOrCreate();
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
            World world = World.GetOrCreate();
            TestSystem system1 = new TestSystem(() => { });
            
            // 2. Call world.AddToUpdate(System1)
            world.AddToUpdate(system1);
            
            // 3. Create System2
            TestSystem system2 = new TestSystem(() => { });
            
            // 4. Call world.AddToUpdate(System2)
            world.AddToUpdate(system2);
            
            // 5. Check queue order by executing
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            
            TestSystem testSystem1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem testSystem2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            world.AddToUpdate(testSystem1);
            world.AddToUpdate(testSystem2);
            
            world.ExecuteUpdate();
            
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
            World world = World.GetOrCreate();
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            int system3Order = -1;
            
            TestSystem system1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            // 2. Add System1 to Update queue
            world.AddToUpdate(system1);
            
            // 3. Add System2 to Update queue
            world.AddToUpdate(system2);
            
            // 4. Create System3
            TestSystem system3 = new TestSystem(() => { system3Order = executionOrder++; });
            
            // 5. Call world.AddToUpdate(System3, order: 0)
            world.AddToUpdate(system3, 0);
            
            // 6. Execute and check order
            world.ExecuteUpdate();
            
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
            World world = World.GetOrCreate();
            int executionOrder = 0;
            int[] orders = new int[4];
            
            TestSystem system1 = new TestSystem(() => { orders[0] = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { orders[1] = executionOrder++; });
            TestSystem system3 = new TestSystem(() => { orders[2] = executionOrder++; });
            
            // 2. Add all to Update queue
            world.AddToUpdate(system1);
            world.AddToUpdate(system2);
            world.AddToUpdate(system3);
            
            // 3. Create System4
            TestSystem system4 = new TestSystem(() => { orders[3] = executionOrder++; });
            
            // 4. Call world.AddToUpdate(System4, order: 1)
            world.AddToUpdate(system4, 1);
            
            // 5. Execute and check order
            world.ExecuteUpdate();
            
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
            World world = World.GetOrCreate();
            TestSystem system = new TestSystem(() => { });
            
            // 2. Attempt to call world.AddToUpdate(System, order: -1)
            bool exceptionThrown = false;
            try
            {
                world.AddToUpdate(system, -1);
            }
            catch (ArgumentOutOfRangeException)
            {
                exceptionThrown = true;
            }
            
            // ArgumentOutOfRangeException is thrown
            Assert(exceptionThrown, "ArgumentOutOfRangeException should be thrown for negative order");
            
            // 3. Attempt to call world.AddToUpdate(System, order: 10) (when queue is empty)
            exceptionThrown = false;
            World emptyWorld = World.GetOrCreate("EmptyTest");
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
            World world = World.GetOrCreate();
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            
            TestSystem system1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            // 2. Call world.AddToFixedUpdate(System1)
            world.AddToFixedUpdate(system1);
            
            // 3. Create System2
            // 4. Call world.AddToFixedUpdate(System2)
            world.AddToFixedUpdate(system2);
            
            // 5. Execute and check order
            world.ExecuteFixedUpdate();
            
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
            World world = World.GetOrCreate();
            int executionOrder = 0;
            int system1Order = -1;
            int system2Order = -1;
            int system3Order = -1;
            
            TestSystem system1 = new TestSystem(() => { system1Order = executionOrder++; });
            TestSystem system2 = new TestSystem(() => { system2Order = executionOrder++; });
            
            // 2. Add System1 to FixedUpdate queue
            world.AddToFixedUpdate(system1);
            
            // 3. Add System2 to FixedUpdate queue
            world.AddToFixedUpdate(system2);
            
            // 4. Create System3
            TestSystem system3 = new TestSystem(() => { system3Order = executionOrder++; });
            
            // 5. Call world.AddToFixedUpdate(System3, order: 0)
            world.AddToFixedUpdate(system3, 0);
            
            // 6. Execute and check order
            world.ExecuteFixedUpdate();
            
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
            World world = World.GetOrCreate();
            TestSystem system = new TestSystem(() => { });
            
            // 2. Attempt to call world.AddToFixedUpdate(System, order: -1)
            bool exceptionThrown = false;
            try
            {
                world.AddToFixedUpdate(system, -1);
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
            
            // 2. Call world.ExecuteOnce(TestSystem)
            World world = World.GetOrCreate();
            world.ExecuteOnce(testSystem);
            
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
            World world = World.GetOrCreate();
            TestSystem system = new TestSystem(() => { });
            
            // 2. Call world.ExecuteOnce(System)
            world.ExecuteOnce(system);
            
            // 3. Execute queues
            int executionCount = 0;
            TestSystem counterSystem = new TestSystem(() => { executionCount++; });
            world.AddToUpdate(counterSystem);
            world.ExecuteUpdate();
            
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
            
            // 2. Call world.ExecuteOnce(TestSystem) three times
            World world = World.GetOrCreate();
            world.ExecuteOnce(testSystem);
            world.ExecuteOnce(testSystem);
            world.ExecuteOnce(testSystem);
            
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
            World world = World.GetOrCreate();
            
            // 2. Call world.ExecuteUpdate()
            // No exception, completes successfully
            world.ExecuteUpdate();
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
            World world = World.GetOrCreate();
            world.AddToUpdate(testSystem);
            
            // 3. Call world.ExecuteUpdate()
            world.ExecuteUpdate();
            
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
            World world = World.GetOrCreate();
            int[] executionOrder = new int[3];
            int order = 0;
            
            TestSystem system1 = new TestSystem(() => { executionOrder[0] = order++; });
            TestSystem system2 = new TestSystem(() => { executionOrder[1] = order++; });
            TestSystem system3 = new TestSystem(() => { executionOrder[2] = order++; });
            
            // 2. Add all to Update queue
            world.AddToUpdate(system1);
            world.AddToUpdate(system2);
            world.AddToUpdate(system3);
            
            // 3. Call world.ExecuteUpdate()
            world.ExecuteUpdate();
            
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
            World world = World.GetOrCreate();
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { throw new Exception("Test exception"); });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 2. Add both to Update queue
            world.AddToUpdate(system1);
            world.AddToUpdate(system2);
            
            // 3. Call world.ExecuteUpdate()
            world.ExecuteUpdate();
            
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
            World world = World.GetOrCreate();
            
            // 2. Call world.ExecuteFixedUpdate()
            // No exception, completes successfully
            world.ExecuteFixedUpdate();
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
            World world = World.GetOrCreate();
            world.AddToFixedUpdate(testSystem);
            
            // 3. Call world.ExecuteFixedUpdate()
            world.ExecuteFixedUpdate();
            
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
            World world = World.GetOrCreate();
            int[] executionOrder = new int[3];
            int order = 0;
            
            TestSystem system1 = new TestSystem(() => { executionOrder[0] = order++; });
            TestSystem system2 = new TestSystem(() => { executionOrder[1] = order++; });
            TestSystem system3 = new TestSystem(() => { executionOrder[2] = order++; });
            
            // 2. Add all to FixedUpdate queue
            world.AddToFixedUpdate(system1);
            world.AddToFixedUpdate(system2);
            world.AddToFixedUpdate(system3);
            
            // 3. Call world.ExecuteFixedUpdate()
            world.ExecuteFixedUpdate();
            
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
            World world = World.GetOrCreate();
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { throw new Exception("Test exception"); });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 2. Add both to FixedUpdate queue
            world.AddToFixedUpdate(system1);
            world.AddToFixedUpdate(system2);
            
            // 3. Call world.ExecuteFixedUpdate()
            world.ExecuteFixedUpdate();
            
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
            World world = World.GetOrCreate();
            bool executed = false;
            TestSystem system = new TestSystem(() => { executed = true; });
            
            // 2. Add to Update queue
            world.AddToUpdate(system);
            
            // 3. Remove from Update queue
            bool removed = world.RemoveFromUpdate(system);
            
            // 4. Execute Update queue
            world.ExecuteUpdate();
            
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
            World world = World.GetOrCreate();
            bool executed = false;
            TestSystem system = new TestSystem(() => { executed = true; });
            
            // 2. Add to FixedUpdate queue
            world.AddToFixedUpdate(system);
            
            // 3. Remove from FixedUpdate queue
            bool removed = world.RemoveFromFixedUpdate(system);
            
            // 4. Execute FixedUpdate queue
            world.ExecuteFixedUpdate();
            
            // 5. Verify system was removed
            Assert(removed, "RemoveFromFixedUpdate should return true");
            Assert(!executed, "System should not execute after removal");
        });
    }
}

