using UnityEngine;
using ArtyECS.Core;
using System.Linq;
using System.Collections;
using System;

/// <summary>
/// Test cases for ArtyECS System Framework functionality (System-000 through System-006 and Play Mode Tests)
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
            // Create instance of System class
            SystemHandler system = new SystemHandler();
            
            // Verify that instance is not null
            AssertNotNull(system, "System instance should not be null");
        });
    }
    
    [ContextMenu("Run Test: System Execute Method")]
    public void Test_System_002()
    {
        string testName = "Test_System_002";
        ExecuteTest(testName, () =>
        {
            // Create System instance
            SystemHandler system = new SystemHandler();
            
            // Call Execute() method
            // Should not throw exception
            try
            {
                system.Execute();
            }
            catch (Exception ex)
            {
                throw new AssertionException($"Execute() should not throw exception: {ex.Message}");
            }
        });
    }
    
    [ContextMenu("Run Test: System Execute Override")]
    public void Test_System_003()
    {
        string testName = "Test_System_003";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem : SystemHandler with overridden Execute()
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // Call Execute()
            testSystem.Execute();
            
            // Check flag
            Assert(flag, "Flag should be true after Execute()");
        });
    }
    
    [ContextMenu("Run Test: System State Support")]
    public void Test_System_004()
    {
        string testName = "Test_System_004";
        ExecuteTest(testName, () =>
        {
            // Create System with instance field
            StatefulSystem system = new StatefulSystem();
            
            // Set field value
            int expectedValue = 42;
            system.FieldValue = expectedValue;
            
            // Verify field value
            AssertEquals(expectedValue, system.FieldValue, "FieldValue should be 42");
        });
    }
    
    [ContextMenu("Run Test: System ToString")]
    public void Test_System_005()
    {
        string testName = "Test_System_005";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem instance
            TestSystem system = new TestSystem(() => { });
            
            // Call ToString()
            string str = system.ToString();
            
            // Verify string contains system type name
            Assert(str.Contains("TestSystem"), "ToString() should contain 'TestSystem'");
        });
    }
    
    // ========== System-001: SystemsRegistry - Basic Structure ==========
    
    [ContextMenu("Run Test: GetGlobalWorld Returns World")]
    public void Test_Registry_001()
    {
        string testName = "Test_Registry_001";
        ExecuteTest(testName, () =>
        {
            // Call SystemsRegistry.GetGlobalWorld()
            World world = SystemsRegistry.GetGlobalWorld();
            
            // Verify world is not null
            AssertNotNull(world, "GetGlobalWorld() should return non-null World");
            
            // Verify world name is "Global"
            AssertEquals("Global", world.Name, "World.Name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: IsWorldInitialized for Global World")]
    public void Test_Registry_002()
    {
        string testName = "Test_Registry_002";
        ExecuteTest(testName, () =>
        {
            // Check IsWorldInitialized() before usage (may be false or true)
            bool beforeUsage = SystemsRegistry.IsWorldInitialized();
            
            // Add system to queue (initializes world)
            SystemHandler system = new SystemHandler();
            SystemsRegistry.AddToUpdate(system);
            
            // Check IsWorldInitialized() after usage
            bool afterUsage = SystemsRegistry.IsWorldInitialized();
            
            // Verify world is initialized after usage
            Assert(afterUsage, "IsWorldInitialized() should be true after usage");
        });
    }
    
    [ContextMenu("Run Test: GetWorldCount")]
    public void Test_Registry_003()
    {
        string testName = "Test_Registry_003";
        ExecuteTest(testName, () =>
        {
            // Get initial world count
            int initialCount = SystemsRegistry.GetWorldCount();
            
            // Create new World("Test")
            World testWorld = new World("Test");
            
            // Add system to new world
            SystemHandler system = new SystemHandler();
            SystemsRegistry.AddToUpdate(system, testWorld);
            
            // Get new world count
            int newCount = SystemsRegistry.GetWorldCount();
            
            // Verify world count increased
            Assert(initialCount >= 0, "Initial count should be >= 0");
            Assert(newCount == initialCount + 1, "New count should be initialCount + 1");
        });
    }
    
    [ContextMenu("Run Test: World Scoped Storage Isolation")]
    public void Test_Registry_004()
    {
        string testName = "Test_Registry_004";
        ExecuteTest(testName, () =>
        {
            // Create World1("World1") and World2("World2")
            World world1 = new World("World1");
            World world2 = new World("World2");
            
            // Create System1 and System2
            SystemHandler system1 = new SystemHandler();
            SystemHandler system2 = new SystemHandler();
            
            // Add System1 to Update queue in World1
            SystemsRegistry.AddToUpdate(system1, world1);
            
            // Add System2 to Update queue in World2
            SystemsRegistry.AddToUpdate(system2, world2);
            
            // Check queues in each world
            var queue1 = SystemsRegistry.GetUpdateQueue(world1);
            var queue2 = SystemsRegistry.GetUpdateQueue(world2);
            
            // Verify systems are isolated by worlds
            Assert(queue1.Contains(system1), "World1 should contain System1");
            Assert(!queue1.Contains(system2), "World1 should not contain System2");
            Assert(!queue2.Contains(system1), "World2 should not contain System1");
            Assert(queue2.Contains(system2), "World2 should contain System2");
        });
    }
    
    // ========== System-002: SystemsRegistry - Update Queue Management ==========
    
    [ContextMenu("Run Test: AddToUpdate Without Order")]
    public void Test_Update_001()
    {
        string testName = "Test_Update_001";
        ExecuteTest(testName, () =>
        {
            // Create System1
            SystemHandler system1 = new SystemHandler();
            
            // Call AddToUpdate(System1)
            SystemsRegistry.AddToUpdate(system1);
            
            // Create System2
            SystemHandler system2 = new SystemHandler();
            
            // Call AddToUpdate(System2)
            SystemsRegistry.AddToUpdate(system2);
            
            // Check queue order
            var queue = SystemsRegistry.GetUpdateQueue();
            AssertEquals(2, queue.Count, "Queue should have 2 systems");
            Assert(queue[0] == system1, "First system should be System1");
            Assert(queue[1] == system2, "Second system should be System2");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate With Order (Insert at Beginning)")]
    public void Test_Update_002()
    {
        string testName = "Test_Update_002";
        ExecuteTest(testName, () =>
        {
            // Create System1, System2
            SystemHandler system1 = new SystemHandler();
            SystemHandler system2 = new SystemHandler();
            
            // Add System1 to Update queue
            SystemsRegistry.AddToUpdate(system1);
            
            // Add System2 to Update queue
            SystemsRegistry.AddToUpdate(system2);
            
            // Create System3
            SystemHandler system3 = new SystemHandler();
            
            // Call AddToUpdate(System3, order: 0)
            SystemsRegistry.AddToUpdate(system3, 0);
            
            // Check queue order
            var queue = SystemsRegistry.GetUpdateQueue();
            AssertEquals(3, queue.Count, "Queue should have 3 systems");
            Assert(queue[0] == system3, "First system should be System3");
            Assert(queue[1] == system1, "Second system should be System1");
            Assert(queue[2] == system2, "Third system should be System2");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate With Order (Insert at Middle)")]
    public void Test_Update_003()
    {
        string testName = "Test_Update_003";
        ExecuteTest(testName, () =>
        {
            // Create System1, System2, System3
            SystemHandler system1 = new SystemHandler();
            SystemHandler system2 = new SystemHandler();
            SystemHandler system3 = new SystemHandler();
            
            // Add all to Update queue
            SystemsRegistry.AddToUpdate(system1);
            SystemsRegistry.AddToUpdate(system2);
            SystemsRegistry.AddToUpdate(system3);
            
            // Create System4
            SystemHandler system4 = new SystemHandler();
            
            // Call AddToUpdate(System4, order: 1)
            SystemsRegistry.AddToUpdate(system4, 1);
            
            // Check queue order
            var queue = SystemsRegistry.GetUpdateQueue();
            AssertEquals(4, queue.Count, "Queue should have 4 systems");
            Assert(queue[0] == system1, "First system should be System1");
            Assert(queue[1] == system4, "Second system should be System4");
            Assert(queue[2] == system2, "Third system should be System2");
            Assert(queue[3] == system3, "Fourth system should be System3");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate With Order (Insert at End)")]
    public void Test_Update_004()
    {
        string testName = "Test_Update_004";
        ExecuteTest(testName, () =>
        {
            // Create System1, System2
            SystemHandler system1 = new SystemHandler();
            SystemHandler system2 = new SystemHandler();
            
            // Add both to Update queue
            SystemsRegistry.AddToUpdate(system1);
            SystemsRegistry.AddToUpdate(system2);
            
            // Create System3
            SystemHandler system3 = new SystemHandler();
            
            // Call AddToUpdate(System3, order: 2)
            SystemsRegistry.AddToUpdate(system3, 2);
            
            // Check queue order
            var queue = SystemsRegistry.GetUpdateQueue();
            AssertEquals(3, queue.Count, "Queue should have 3 systems");
            Assert(queue[2] == system3, "Third system should be System3");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate Null System Throws Exception")]
    public void Test_Update_005()
    {
        string testName = "Test_Update_005";
        ExecuteTest(testName, () =>
        {
            // Attempt to call AddToUpdate(null)
            bool exceptionThrown = false;
            try
            {
                SystemsRegistry.AddToUpdate(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ArgumentNullException should be thrown");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate Invalid Order Throws Exception")]
    public void Test_Update_006()
    {
        string testName = "Test_Update_006";
        ExecuteTest(testName, () =>
        {
            // Create System
            SystemHandler system = new SystemHandler();
            
            // Attempt to call AddToUpdate(System, order: -1)
            bool negativeExceptionThrown = false;
            try
            {
                SystemsRegistry.AddToUpdate(system, -1);
            }
            catch (ArgumentOutOfRangeException)
            {
                negativeExceptionThrown = true;
            }
            
            Assert(negativeExceptionThrown, "ArgumentOutOfRangeException should be thrown for negative order");
            
            // Attempt to call AddToUpdate(System, order: 10) (when queue is empty)
            bool outOfRangeExceptionThrown = false;
            try
            {
                SystemsRegistry.AddToUpdate(system, 10);
            }
            catch (ArgumentOutOfRangeException)
            {
                outOfRangeExceptionThrown = true;
            }
            
            Assert(outOfRangeExceptionThrown, "ArgumentOutOfRangeException should be thrown for order > queue count");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate Extension Method")]
    public void Test_Update_007()
    {
        string testName = "Test_Update_007";
        ExecuteTest(testName, () =>
        {
            // Create System
            SystemHandler system = new SystemHandler();
            
            // Call system.AddToUpdate()
            system.AddToUpdate();
            
            // Check queue
            var queue = SystemsRegistry.GetUpdateQueue();
            Assert(queue.Contains(system), "Queue should contain system");
        });
    }
    
    [ContextMenu("Run Test: AddToUpdate With World Parameter")]
    public void Test_Update_008()
    {
        string testName = "Test_Update_008";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create System
            SystemHandler system = new SystemHandler();
            
            // Add System to testWorld Update queue
            SystemsRegistry.AddToUpdate(system, testWorld);
            
            // Check queue in testWorld
            var testQueue = SystemsRegistry.GetUpdateQueue(testWorld);
            Assert(testQueue.Contains(system), "TestWorld queue should contain system");
            
            // Check queue in global world
            var globalQueue = SystemsRegistry.GetUpdateQueue(null);
            Assert(!globalQueue.Contains(system), "Global queue should not contain system");
        });
    }
    
    // ========== System-003: SystemsRegistry - FixedUpdate Queue Management ==========
    
    [ContextMenu("Run Test: AddToFixedUpdate Without Order")]
    public void Test_FixedUpdate_001()
    {
        string testName = "Test_FixedUpdate_001";
        ExecuteTest(testName, () =>
        {
            // Create System1
            SystemHandler system1 = new SystemHandler();
            
            // Call AddToFixedUpdate(System1)
            SystemsRegistry.AddToFixedUpdate(system1);
            
            // Create System2
            SystemHandler system2 = new SystemHandler();
            
            // Call AddToFixedUpdate(System2)
            SystemsRegistry.AddToFixedUpdate(system2);
            
            // Check queue order
            var queue = SystemsRegistry.GetFixedUpdateQueue();
            AssertEquals(2, queue.Count, "Queue should have 2 systems");
            Assert(queue[0] == system1, "First system should be System1");
            Assert(queue[1] == system2, "Second system should be System2");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate With Order (Insert at Beginning)")]
    public void Test_FixedUpdate_002()
    {
        string testName = "Test_FixedUpdate_002";
        ExecuteTest(testName, () =>
        {
            // Create System1, System2
            SystemHandler system1 = new SystemHandler();
            SystemHandler system2 = new SystemHandler();
            
            // Add System1 to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(system1);
            
            // Add System2 to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(system2);
            
            // Create System3
            SystemHandler system3 = new SystemHandler();
            
            // Call AddToFixedUpdate(System3, order: 0)
            SystemsRegistry.AddToFixedUpdate(system3, 0);
            
            // Check queue order
            var queue = SystemsRegistry.GetFixedUpdateQueue();
            AssertEquals(3, queue.Count, "Queue should have 3 systems");
            Assert(queue[0] == system3, "First system should be System3");
            Assert(queue[1] == system1, "Second system should be System1");
            Assert(queue[2] == system2, "Third system should be System2");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate With Order (Insert at Middle)")]
    public void Test_FixedUpdate_003()
    {
        string testName = "Test_FixedUpdate_003";
        ExecuteTest(testName, () =>
        {
            // Create System1, System2, System3
            SystemHandler system1 = new SystemHandler();
            SystemHandler system2 = new SystemHandler();
            SystemHandler system3 = new SystemHandler();
            
            // Add all to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(system1);
            SystemsRegistry.AddToFixedUpdate(system2);
            SystemsRegistry.AddToFixedUpdate(system3);
            
            // Create System4
            SystemHandler system4 = new SystemHandler();
            
            // Call AddToFixedUpdate(System4, order: 1)
            SystemsRegistry.AddToFixedUpdate(system4, 1);
            
            // Check queue order
            var queue = SystemsRegistry.GetFixedUpdateQueue();
            AssertEquals(4, queue.Count, "Queue should have 4 systems");
            Assert(queue[0] == system1, "First system should be System1");
            Assert(queue[1] == system4, "Second system should be System4");
            Assert(queue[2] == system2, "Third system should be System2");
            Assert(queue[3] == system3, "Fourth system should be System3");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate Null System Throws Exception")]
    public void Test_FixedUpdate_004()
    {
        string testName = "Test_FixedUpdate_004";
        ExecuteTest(testName, () =>
        {
            // Attempt to call AddToFixedUpdate(null)
            bool exceptionThrown = false;
            try
            {
                SystemsRegistry.AddToFixedUpdate(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ArgumentNullException should be thrown");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate Invalid Order Throws Exception")]
    public void Test_FixedUpdate_005()
    {
        string testName = "Test_FixedUpdate_005";
        ExecuteTest(testName, () =>
        {
            // Create System
            SystemHandler system = new SystemHandler();
            
            // Attempt to call AddToFixedUpdate(System, order: -1)
            bool negativeExceptionThrown = false;
            try
            {
                SystemsRegistry.AddToFixedUpdate(system, -1);
            }
            catch (ArgumentOutOfRangeException)
            {
                negativeExceptionThrown = true;
            }
            
            Assert(negativeExceptionThrown, "ArgumentOutOfRangeException should be thrown for negative order");
            
            // Attempt to call AddToFixedUpdate(System, order: 10) (when queue is empty)
            bool outOfRangeExceptionThrown = false;
            try
            {
                SystemsRegistry.AddToFixedUpdate(system, 10);
            }
            catch (ArgumentOutOfRangeException)
            {
                outOfRangeExceptionThrown = true;
            }
            
            Assert(outOfRangeExceptionThrown, "ArgumentOutOfRangeException should be thrown for order > queue count");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate Extension Method")]
    public void Test_FixedUpdate_006()
    {
        string testName = "Test_FixedUpdate_006";
        ExecuteTest(testName, () =>
        {
            // Create System
            SystemHandler system = new SystemHandler();
            
            // Call system.AddToFixedUpdate()
            system.AddToFixedUpdate();
            
            // Check queue
            var queue = SystemsRegistry.GetFixedUpdateQueue();
            Assert(queue.Contains(system), "Queue should contain system");
        });
    }
    
    [ContextMenu("Run Test: AddToFixedUpdate With World Parameter")]
    public void Test_FixedUpdate_007()
    {
        string testName = "Test_FixedUpdate_007";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create System
            SystemHandler system = new SystemHandler();
            
            // Add System to testWorld FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(system, testWorld);
            
            // Check queue in testWorld
            var testQueue = SystemsRegistry.GetFixedUpdateQueue(testWorld);
            Assert(testQueue.Contains(system), "TestWorld queue should contain system");
            
            // Check queue in global world
            var globalQueue = SystemsRegistry.GetFixedUpdateQueue(null);
            Assert(!globalQueue.Contains(system), "Global queue should not contain system");
        });
    }
    
    // ========== System-004: SystemsRegistry - Manual Execution ==========
    
    [ContextMenu("Run Test: ExecuteOnce Executes System")]
    public void Test_ExecuteOnce_001()
    {
        string testName = "Test_ExecuteOnce_001";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem with Execute() that sets flag
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // Call ExecuteOnce(TestSystem)
            SystemsRegistry.ExecuteOnce(testSystem);
            
            // Check flag
            Assert(flag, "Flag should be true after ExecuteOnce()");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce Does Not Add to Queue")]
    public void Test_ExecuteOnce_002()
    {
        string testName = "Test_ExecuteOnce_002";
        ExecuteTest(testName, () =>
        {
            // Create System
            SystemHandler system = new SystemHandler();
            
            // Call ExecuteOnce(System)
            SystemsRegistry.ExecuteOnce(system);
            
            // Check Update queue
            var updateQueue = SystemsRegistry.GetUpdateQueue();
            Assert(!updateQueue.Contains(system), "Update queue should not contain system");
            
            // Check FixedUpdate queue
            var fixedUpdateQueue = SystemsRegistry.GetFixedUpdateQueue();
            Assert(!fixedUpdateQueue.Contains(system), "FixedUpdate queue should not contain system");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce Multiple Times")]
    public void Test_ExecuteOnce_003()
    {
        string testName = "Test_ExecuteOnce_003";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem with counter
            int counter = 0;
            TestSystem testSystem = new TestSystem(() => { counter++; });
            
            // Call ExecuteOnce(TestSystem) three times
            SystemsRegistry.ExecuteOnce(testSystem);
            SystemsRegistry.ExecuteOnce(testSystem);
            SystemsRegistry.ExecuteOnce(testSystem);
            
            // Check counter
            AssertEquals(3, counter, "Counter should be 3");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce Null System Throws Exception")]
    public void Test_ExecuteOnce_004()
    {
        string testName = "Test_ExecuteOnce_004";
        ExecuteTest(testName, () =>
        {
            // Attempt to call ExecuteOnce(null)
            bool exceptionThrown = false;
            try
            {
                SystemsRegistry.ExecuteOnce(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ArgumentNullException should be thrown");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce Extension Method")]
    public void Test_ExecuteOnce_005()
    {
        string testName = "Test_ExecuteOnce_005";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem with Execute() that sets flag
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // Call system.ExecuteOnce()
            testSystem.ExecuteOnce();
            
            // Check flag
            Assert(flag, "Flag should be true after ExecuteOnce()");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce Exception Propagation")]
    public void Test_ExecuteOnce_006()
    {
        string testName = "Test_ExecuteOnce_006";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem that throws exception in Execute()
            TestSystem testSystem = new TestSystem(() => { throw new Exception("Test exception"); });
            
            // Attempt to call ExecuteOnce(TestSystem)
            bool exceptionThrown = false;
            try
            {
                SystemsRegistry.ExecuteOnce(testSystem);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                Assert(ex.Message.Contains("Test exception"), "Exception message should contain 'Test exception'");
            }
            
            Assert(exceptionThrown, "Exception should propagate to caller");
        });
    }
    
    // ========== System-005: SystemsRegistry - Queue Execution (Sync) ==========
    
    [ContextMenu("Run Test: ExecuteUpdate Empty Queue")]
    public void Test_ExecuteUpdate_001()
    {
        string testName = "Test_ExecuteUpdate_001";
        ExecuteTest(testName, () =>
        {
            // Ensure Update queue is empty
            var queue = SystemsRegistry.GetUpdateQueue();
            queue.Clear();
            
            // Call ExecuteUpdate()
            // Should not throw exception
            try
            {
                SystemsRegistry.ExecuteUpdate();
            }
            catch (Exception ex)
            {
                throw new AssertionException($"ExecuteUpdate() should not throw exception with empty queue: {ex.Message}");
            }
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate Single System")]
    public void Test_ExecuteUpdate_002()
    {
        string testName = "Test_ExecuteUpdate_002";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem with Execute() that sets flag
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // Add TestSystem to Update queue
            SystemsRegistry.AddToUpdate(testSystem);
            
            // Call ExecuteUpdate()
            SystemsRegistry.ExecuteUpdate();
            
            // Check flag
            Assert(flag, "Flag should be true after ExecuteUpdate()");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate Multiple Systems in Order")]
    public void Test_ExecuteUpdate_003()
    {
        string testName = "Test_ExecuteUpdate_003";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem1, TestSystem2, TestSystem3 with execution order tracking
            int[] executionOrder = new int[3];
            int currentOrder = 0;
            
            TestSystem system1 = new TestSystem(() => { executionOrder[0] = currentOrder++; });
            TestSystem system2 = new TestSystem(() => { executionOrder[1] = currentOrder++; });
            TestSystem system3 = new TestSystem(() => { executionOrder[2] = currentOrder++; });
            
            // Add all to Update queue
            SystemsRegistry.AddToUpdate(system1);
            SystemsRegistry.AddToUpdate(system2);
            SystemsRegistry.AddToUpdate(system3);
            
            // Call ExecuteUpdate()
            SystemsRegistry.ExecuteUpdate();
            
            // Check execution order
            AssertEquals(0, executionOrder[0], "System1 should execute at order 0");
            AssertEquals(1, executionOrder[1], "System2 should execute at order 1");
            AssertEquals(2, executionOrder[2], "System3 should execute at order 2");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate Continues After Exception")]
    public void Test_ExecuteUpdate_004()
    {
        string testName = "Test_ExecuteUpdate_004";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem1 (throws exception), TestSystem2 (sets flag)
            bool flag = false;
            TestSystem system1 = new TestSystem(() => { throw new Exception("System1 exception"); });
            TestSystem system2 = new TestSystem(() => { flag = true; });
            
            // Add both to Update queue
            SystemsRegistry.AddToUpdate(system1);
            SystemsRegistry.AddToUpdate(system2);
            
            // Call ExecuteUpdate()
            // Should not throw exception, should continue execution
            try
            {
                SystemsRegistry.ExecuteUpdate();
            }
            catch (Exception ex)
            {
                throw new AssertionException($"ExecuteUpdate() should continue after exception: {ex.Message}");
            }
            
            // Check flag
            Assert(flag, "Flag should be true (System2 should execute despite System1 exception)");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate With World Parameter")]
    public void Test_ExecuteUpdate_005()
    {
        string testName = "Test_ExecuteUpdate_005";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create System1 in global world
            bool system1Executed = false;
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            SystemsRegistry.AddToUpdate(system1, null);
            
            // Create System2 in testWorld
            bool system2Executed = false;
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            SystemsRegistry.AddToUpdate(system2, testWorld);
            
            // Call ExecuteUpdate(testWorld)
            SystemsRegistry.ExecuteUpdate(testWorld);
            
            // Check execution flags
            Assert(!system1Executed, "System1 should not be executed");
            Assert(system2Executed, "System2 should be executed");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Empty Queue")]
    public void Test_ExecuteFixedUpdate_001()
    {
        string testName = "Test_ExecuteFixedUpdate_001";
        ExecuteTest(testName, () =>
        {
            // Ensure FixedUpdate queue is empty
            var queue = SystemsRegistry.GetFixedUpdateQueue();
            queue.Clear();
            
            // Call ExecuteFixedUpdate()
            // Should not throw exception
            try
            {
                SystemsRegistry.ExecuteFixedUpdate();
            }
            catch (Exception ex)
            {
                throw new AssertionException($"ExecuteFixedUpdate() should not throw exception with empty queue: {ex.Message}");
            }
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Single System")]
    public void Test_ExecuteFixedUpdate_002()
    {
        string testName = "Test_ExecuteFixedUpdate_002";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem with Execute() that sets flag
            bool flag = false;
            TestSystem testSystem = new TestSystem(() => { flag = true; });
            
            // Add TestSystem to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(testSystem);
            
            // Call ExecuteFixedUpdate()
            SystemsRegistry.ExecuteFixedUpdate();
            
            // Check flag
            Assert(flag, "Flag should be true after ExecuteFixedUpdate()");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Multiple Systems in Order")]
    public void Test_ExecuteFixedUpdate_003()
    {
        string testName = "Test_ExecuteFixedUpdate_003";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem1, TestSystem2, TestSystem3 with execution order tracking
            int[] executionOrder = new int[3];
            int currentOrder = 0;
            
            TestSystem system1 = new TestSystem(() => { executionOrder[0] = currentOrder++; });
            TestSystem system2 = new TestSystem(() => { executionOrder[1] = currentOrder++; });
            TestSystem system3 = new TestSystem(() => { executionOrder[2] = currentOrder++; });
            
            // Add all to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(system1);
            SystemsRegistry.AddToFixedUpdate(system2);
            SystemsRegistry.AddToFixedUpdate(system3);
            
            // Call ExecuteFixedUpdate()
            SystemsRegistry.ExecuteFixedUpdate();
            
            // Check execution order
            AssertEquals(0, executionOrder[0], "System1 should execute at order 0");
            AssertEquals(1, executionOrder[1], "System2 should execute at order 1");
            AssertEquals(2, executionOrder[2], "System3 should execute at order 2");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate Continues After Exception")]
    public void Test_ExecuteFixedUpdate_004()
    {
        string testName = "Test_ExecuteFixedUpdate_004";
        ExecuteTest(testName, () =>
        {
            // Create TestSystem1 (throws exception), TestSystem2 (sets flag)
            bool flag = false;
            TestSystem system1 = new TestSystem(() => { throw new Exception("System1 exception"); });
            TestSystem system2 = new TestSystem(() => { flag = true; });
            
            // Add both to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(system1);
            SystemsRegistry.AddToFixedUpdate(system2);
            
            // Call ExecuteFixedUpdate()
            // Should not throw exception, should continue execution
            try
            {
                SystemsRegistry.ExecuteFixedUpdate();
            }
            catch (Exception ex)
            {
                throw new AssertionException($"ExecuteFixedUpdate() should continue after exception: {ex.Message}");
            }
            
            // Check flag
            Assert(flag, "Flag should be true (System2 should execute despite System1 exception)");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate With World Parameter")]
    public void Test_ExecuteFixedUpdate_005()
    {
        string testName = "Test_ExecuteFixedUpdate_005";
        ExecuteTest(testName, () =>
        {
            // Create World("Test")
            World testWorld = new World("Test");
            
            // Create System1 in global world
            bool system1Executed = false;
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            SystemsRegistry.AddToFixedUpdate(system1, null);
            
            // Create System2 in testWorld
            bool system2Executed = false;
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            SystemsRegistry.AddToFixedUpdate(system2, testWorld);
            
            // Call ExecuteFixedUpdate(testWorld)
            SystemsRegistry.ExecuteFixedUpdate(testWorld);
            
            // Check execution flags
            Assert(!system1Executed, "System1 should not be executed");
            Assert(system2Executed, "System2 should be executed");
        });
    }
    
    // ========== System-006: System Execution Integration with Unity ==========
    
    [ContextMenu("Run Test: UpdateProvider Auto-Creation")]
    public void Test_UpdateProvider_001()
    {
        string testName = "Test_UpdateProvider_001";
        ExecuteTest(testName, () =>
        {
            // Ensure no UpdateProvider exists (clear any existing)
            var existing = GameObject.Find("UpdateProvider");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }
            
            // Call World.CreateEntity() (this should create UpdateProvider)
            Entity entity = World.CreateEntity();
            
            // Check for UpdateProvider GameObject
            var updateProvider = GameObject.Find("UpdateProvider");
            AssertNotNull(updateProvider, "UpdateProvider GameObject should be created");
            
            var instance = UpdateProvider.Instance;
            AssertNotNull(instance, "UpdateProvider.Instance should not be null");
        });
    }
    
    [ContextMenu("Run Test: UpdateProvider Singleton")]
    public void Test_UpdateProvider_002()
    {
        string testName = "Test_UpdateProvider_002";
        ExecuteTest(testName, () =>
        {
            // Create first entity (creates UpdateProvider)
            Entity entity1 = World.CreateEntity();
            
            // Create second entity
            Entity entity2 = World.CreateEntity();
            
            // Check UpdateProvider count
            var providers = GameObject.FindObjectsOfType<UpdateProvider>();
            AssertEquals(1, providers.Length, "Should have only one UpdateProvider");
        });
    }
    
    // Note: Test_UpdateProvider_003 (Persistence) requires scene loading, which is complex for unit tests
    // This would be better as an integration test
    
    // Note: Test_UpdateProvider_004, 005, 006 require Play Mode and frame waiting
    // These are handled in Play Mode tests section
    
    // ========== System Play Mode Tests (Update/FixedUpdate with Components) ==========
    // Note: These tests are designed for Play Mode but can be run manually via ExecuteUpdate/ExecuteFixedUpdate
    
    [ContextMenu("Run Test: System Increments Component Value in Update")]
    public void Test_PlayMode_001()
    {
        string testName = "Test_PlayMode_001";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add CounterComponent with Value=0
            ComponentsRegistry.AddComponent(entity, new CounterComponent { Value = 0 });
            
            // Create IncrementSystem that increments CounterComponent.Value in Execute()
            IncrementSystem incrementSystem = new IncrementSystem();
            
            // Add IncrementSystem to Update queue
            SystemsRegistry.AddToUpdate(incrementSystem);
            
            // Execute Update 2 times (simulating 2 frames)
            SystemsRegistry.ExecuteUpdate();
            SystemsRegistry.ExecuteUpdate();
            
            // Check CounterComponent.Value
            var counter = ComponentsRegistry.GetComponent<CounterComponent>(entity);
            Assert(counter.HasValue, "CounterComponent should exist");
            Assert(counter.Value.Value >= 2, "CounterComponent.Value should be >= 2");
        });
    }
    
    [ContextMenu("Run Test: System Modifies Multiple Components in Update")]
    public void Test_PlayMode_002()
    {
        string testName = "Test_PlayMode_002";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Position(X=0, Y=0) and Velocity(X=1, Y=1)
            ComponentsRegistry.AddComponent(entity, new Position { X = 0f, Y = 0f, Z = 0f });
            ComponentsRegistry.AddComponent(entity, new Velocity { X = 1f, Y = 1f, Z = 0f });
            
            // Create MovementSystem that adds Velocity to Position in Execute()
            MovementSystem movementSystem = new MovementSystem();
            
            // Add MovementSystem to Update queue
            SystemsRegistry.AddToUpdate(movementSystem);
            
            // Execute Update 2 times (simulating 2 frames)
            SystemsRegistry.ExecuteUpdate();
            SystemsRegistry.ExecuteUpdate();
            
            // Check Position values
            var position = ComponentsRegistry.GetComponent<Position>(entity);
            Assert(position.HasValue, "Position should exist");
            Assert(position.Value.X >= 2f, "Position.X should be >= 2");
            Assert(position.Value.Y >= 2f, "Position.Y should be >= 2");
        });
    }
    
    [ContextMenu("Run Test: System Queries Components in Update")]
    public void Test_PlayMode_003()
    {
        string testName = "Test_PlayMode_003";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add Health to all with Amount=100
            ComponentsRegistry.AddComponent(entity1, new Health { Amount = 100f });
            ComponentsRegistry.AddComponent(entity2, new Health { Amount = 100f });
            ComponentsRegistry.AddComponent(entity3, new Health { Amount = 100f });
            
            // Create HealthSystem that decrements all Health.Amount by 1 in Execute()
            HealthSystem healthSystem = new HealthSystem();
            
            // Add HealthSystem to Update queue
            SystemsRegistry.AddToUpdate(healthSystem);
            
            // Execute Update 2 times (simulating 2 frames)
            SystemsRegistry.ExecuteUpdate();
            SystemsRegistry.ExecuteUpdate();
            
            // Check Health values
            var health1 = ComponentsRegistry.GetComponent<Health>(entity1);
            var health2 = ComponentsRegistry.GetComponent<Health>(entity2);
            var health3 = ComponentsRegistry.GetComponent<Health>(entity3);
            
            Assert(health1.HasValue, "Entity1 Health should exist");
            AssertEquals(98f, health1.Value.Amount, "Entity1 Health.Amount should be 98");
            Assert(health2.HasValue, "Entity2 Health should exist");
            AssertEquals(98f, health2.Value.Amount, "Entity2 Health.Amount should be 98");
            Assert(health3.HasValue, "Entity3 Health should exist");
            AssertEquals(98f, health3.Value.Amount, "Entity3 Health.Amount should be 98");
        });
    }
    
    [ContextMenu("Run Test: System Uses ModifiableComponents in Update")]
    public void Test_PlayMode_004()
    {
        string testName = "Test_PlayMode_004";
        ExecuteTest(testName, () =>
        {
            // Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // Add Health to all with Amount=100
            ComponentsRegistry.AddComponent(entity1, new Health { Amount = 100f });
            ComponentsRegistry.AddComponent(entity2, new Health { Amount = 100f });
            ComponentsRegistry.AddComponent(entity3, new Health { Amount = 100f });
            
            // Create HealthSystem that uses GetModifiableComponents<Health>() and modifies values
            ModifiableHealthSystem healthSystem = new ModifiableHealthSystem();
            
            // Add HealthSystem to Update queue
            SystemsRegistry.AddToUpdate(healthSystem);
            
            // Execute Update 2 times (simulating 2 frames)
            SystemsRegistry.ExecuteUpdate();
            SystemsRegistry.ExecuteUpdate();
            
            // Check Health values
            var health1 = ComponentsRegistry.GetComponent<Health>(entity1);
            var health2 = ComponentsRegistry.GetComponent<Health>(entity2);
            var health3 = ComponentsRegistry.GetComponent<Health>(entity3);
            
            Assert(health1.HasValue, "Entity1 Health should exist");
            Assert(health2.HasValue, "Entity2 Health should exist");
            Assert(health3.HasValue, "Entity3 Health should exist");
            
            // All should be reduced by 2 (2 frames * 1 per frame)
            Assert(health1.Value.Amount <= 98f, "Entity1 Health should be reduced");
            Assert(health2.Value.Amount <= 98f, "Entity2 Health should be reduced");
            Assert(health3.Value.Amount <= 98f, "Entity3 Health should be reduced");
        });
    }
    
    [ContextMenu("Run Test: System Increments Component Value in FixedUpdate")]
    public void Test_PlayMode_005()
    {
        string testName = "Test_PlayMode_005";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add CounterComponent with Value=0
            ComponentsRegistry.AddComponent(entity, new CounterComponent { Value = 0 });
            
            // Create IncrementSystem that increments CounterComponent.Value in Execute()
            IncrementSystem incrementSystem = new IncrementSystem();
            
            // Add IncrementSystem to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(incrementSystem);
            
            // Execute FixedUpdate 2 times (simulating 2 FixedUpdate cycles)
            SystemsRegistry.ExecuteFixedUpdate();
            SystemsRegistry.ExecuteFixedUpdate();
            
            // Check CounterComponent.Value
            var counter = ComponentsRegistry.GetComponent<CounterComponent>(entity);
            Assert(counter.HasValue, "CounterComponent should exist");
            Assert(counter.Value.Value >= 2, "CounterComponent.Value should be >= 2");
        });
    }
    
    [ContextMenu("Run Test: System Physics in FixedUpdate")]
    public void Test_PlayMode_006()
    {
        string testName = "Test_PlayMode_006";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Position(X=0, Y=0), Velocity(X=0, Y=0), Acceleration(X=1, Y=1)
            ComponentsRegistry.AddComponent(entity, new Position { X = 0f, Y = 0f, Z = 0f });
            ComponentsRegistry.AddComponent(entity, new Velocity { X = 0f, Y = 0f, Z = 0f });
            ComponentsRegistry.AddComponent(entity, new Acceleration { X = 1f, Y = 1f, Z = 0f });
            
            // Create PhysicsSystem that adds Acceleration to Velocity, then Velocity to Position in Execute()
            PhysicsSystem physicsSystem = new PhysicsSystem();
            
            // Add PhysicsSystem to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(physicsSystem);
            
            // Execute FixedUpdate 2 times (simulating 2 FixedUpdate cycles)
            SystemsRegistry.ExecuteFixedUpdate();
            SystemsRegistry.ExecuteFixedUpdate();
            
            // Check Position and Velocity values
            var velocity = ComponentsRegistry.GetComponent<Velocity>(entity);
            var position = ComponentsRegistry.GetComponent<Position>(entity);
            
            Assert(velocity.HasValue, "Velocity should exist");
            Assert(velocity.Value.X >= 2f, "Velocity.X should be >= 2");
            Assert(position.HasValue, "Position should exist");
            Assert(position.Value.X >= 2f, "Position.X should be >= 2");
        });
    }
    
    [ContextMenu("Run Test: Multiple Systems in Update Order")]
    public void Test_PlayMode_007()
    {
        string testName = "Test_PlayMode_007";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add CounterComponent with Value=0
            ComponentsRegistry.AddComponent(entity, new CounterComponent { Value = 0 });
            
            // Create System1 that sets Value=1
            SetValueSystem system1 = new SetValueSystem(1);
            
            // Create System2 that increments Value
            IncrementSystem system2 = new IncrementSystem();
            
            // Add System1 to Update queue at order 0
            SystemsRegistry.AddToUpdate(system1, 0);
            
            // Add System2 to Update queue at order 1
            SystemsRegistry.AddToUpdate(system2, 1);
            
            // Execute Update 1 time (simulating 1 frame)
            SystemsRegistry.ExecuteUpdate();
            
            // Check CounterComponent.Value
            var counter = ComponentsRegistry.GetComponent<CounterComponent>(entity);
            Assert(counter.HasValue, "CounterComponent should exist");
            // System1 sets to 1, then System2 increments to 2
            AssertEquals(2, counter.Value.Value, "CounterComponent.Value should be 2");
        });
    }
    
    [ContextMenu("Run Test: Update and FixedUpdate Separation")]
    public void Test_PlayMode_008()
    {
        string testName = "Test_PlayMode_008";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add UpdateCounter(Value=0) and FixedUpdateCounter(Value=0)
            ComponentsRegistry.AddComponent(entity, new UpdateCounter { Value = 0 });
            ComponentsRegistry.AddComponent(entity, new FixedUpdateCounter { Value = 0 });
            
            // Create UpdateSystem that increments UpdateCounter
            UpdateCounterSystem updateSystem = new UpdateCounterSystem();
            
            // Create FixedUpdateSystem that increments FixedUpdateCounter
            FixedUpdateCounterSystem fixedUpdateSystem = new FixedUpdateCounterSystem();
            
            // Add UpdateSystem to Update queue
            SystemsRegistry.AddToUpdate(updateSystem);
            
            // Add FixedUpdateSystem to FixedUpdate queue
            SystemsRegistry.AddToFixedUpdate(fixedUpdateSystem);
            
            // Execute Update 3 times
            SystemsRegistry.ExecuteUpdate();
            SystemsRegistry.ExecuteUpdate();
            SystemsRegistry.ExecuteUpdate();
            
            // Execute FixedUpdate 2 times
            SystemsRegistry.ExecuteFixedUpdate();
            SystemsRegistry.ExecuteFixedUpdate();
            
            // Check both counters
            var updateCounter = ComponentsRegistry.GetComponent<UpdateCounter>(entity);
            var fixedUpdateCounter = ComponentsRegistry.GetComponent<FixedUpdateCounter>(entity);
            
            Assert(updateCounter.HasValue, "UpdateCounter should exist");
            Assert(updateCounter.Value.Value > 0, "UpdateCounter value should be > 0");
            AssertEquals(3, updateCounter.Value.Value, "UpdateCounter should be 3");
            
            Assert(fixedUpdateCounter.HasValue, "FixedUpdateCounter should exist");
            Assert(fixedUpdateCounter.Value.Value > 0, "FixedUpdateCounter value should be > 0");
            AssertEquals(2, fixedUpdateCounter.Value.Value, "FixedUpdateCounter should be 2");
        });
    }
    
    [ContextMenu("Run Test: System Removes Component in Update")]
    public void Test_PlayMode_009()
    {
        string testName = "Test_PlayMode_009";
        ExecuteTest(testName, () =>
        {
            // Create Entity
            Entity entity = World.CreateEntity();
            
            // Add Health(Amount=0) and Dead components
            ComponentsRegistry.AddComponent(entity, new Health { Amount = 0f });
            ComponentsRegistry.AddComponent(entity, new Dead());
            
            // Create CleanupSystem that removes Dead component when Health.Amount <= 0
            CleanupSystem cleanupSystem = new CleanupSystem();
            cleanupSystem.SetEntityToCleanup(entity); // Set entity for cleanup
            
            // Add CleanupSystem to Update queue
            SystemsRegistry.AddToUpdate(cleanupSystem);
            
            // Execute Update 1 time (simulating 1 frame)
            SystemsRegistry.ExecuteUpdate();
            
            // Check Dead component
            var dead = ComponentsRegistry.GetComponent<Dead>(entity);
            Assert(!dead.HasValue, "Dead component should be removed");
        });
    }
    
    [ContextMenu("Run Test: System Creates Entity in Update")]
    public void Test_PlayMode_010()
    {
        string testName = "Test_PlayMode_010";
        ExecuteTest(testName, () =>
        {
            // Create Entity1 with Spawner component
            Entity entity1 = World.CreateEntity();
            ComponentsRegistry.AddComponent(entity1, new Spawner { SpawnCount = 1 });
            
            // Create SpawnSystem that creates new entity when Spawner.SpawnCount > 0
            SpawnSystem spawnSystem = new SpawnSystem();
            
            // Add SpawnSystem to Update queue
            SystemsRegistry.AddToUpdate(spawnSystem);
            
            // Get initial entity count
            int initialCount = EntityPool.GetAllocatedCount();
            
            // Execute Update 1 time (simulating 1 frame)
            SystemsRegistry.ExecuteUpdate();
            
            // Check entity count
            int newCount = EntityPool.GetAllocatedCount();
            Assert(newCount > initialCount, "Entity count should increase");
        });
    }
    
}

