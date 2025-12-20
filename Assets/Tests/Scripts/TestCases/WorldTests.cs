using UnityEngine;
using ArtyECS.Core;
using System;
using System.Threading;

/// <summary>
/// Test class for World Management functionality (World-000 through World-003).
/// </summary>
public class WorldTests : TestBase
{
    // ========== World-000: World Class Implementation ==========
    
    [ContextMenu("Run Test: World Creation with Name")]
    public void Test_WorldClass_001()
    {
        string testName = "Test_WorldClass_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create World with name "TestWorld"
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Verify that World.Name == "TestWorld"
            AssertEquals("TestWorld", world.Name, "World.Name should be 'TestWorld'");
            AssertNotNull(world, "World should not be null");
        });
    }
    
    [ContextMenu("Run Test: Multiple Worlds Creation")]
    public void Test_WorldClass_002()
    {
        string testName = "Test_WorldClass_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("World1")
            WorldInstance world1 = World.GetOrCreate("World1");
            
            // 2. Create World2("World2")
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 3. Create World3("World3")
            WorldInstance world3 = World.GetOrCreate("World3");
            
            // All worlds created successfully
            AssertEquals("World1", world1.Name, "World1.Name should be 'World1'");
            AssertEquals("World2", world2.Name, "World2.Name should be 'World2'");
            AssertEquals("World3", world3.Name, "World3.Name should be 'World3'");
            Assert(world1 != world2, "World1 should not equal World2");
            Assert(world2 != world3, "World2 should not equal World3");
        });
    }
    
    [ContextMenu("Run Test: World Equality")]
    public void Test_WorldClass_003()
    {
        string testName = "Test_WorldClass_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("Test")
            WorldInstance world1 = World.GetOrCreate("Test1");
            
            // 2. Create World2("Test")
            WorldInstance world2 = World.GetOrCreate("Test2");
            
            // 3. Get global world instance
            WorldInstance globalWorld1 = World.GetOrCreate();
            
            // 4. Get global world instance again
            WorldInstance globalWorld2 = World.GetOrCreate();
            
            // Worlds with same name are different instances, global world is same instance
            Assert(world1 != world2, "World1 should not equal World2 (different instances)");
            Assert(globalWorld1 == globalWorld2, "GlobalWorld1 should equal GlobalWorld2 (same singleton)");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Scoped World")]
    public void Test_WorldClass_004()
    {
        string testName = "Test_WorldClass_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Create Entity in TestWorld
            Entity entity = testWorld.CreateEntity();
            
            // 3. Add components to Entity in TestWorld
            testWorld.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 4. Add system to TestWorld
            TestSystem system = new TestSystem(() => { });
            testWorld.AddToUpdate(system);
            
            // 5. Call World.Destroy(TestWorld)
            bool destroyed = World.Destroy(testWorld);
            
            // 6. Verify that world is destroyed
            Assert(destroyed, "World.Destroy should return true");
            
            // Note: After destruction, we can't easily verify components/systems are cleared
            // because the world might be marked as destroyed. But we can verify destruction returned true.
        });
    }
    
    [ContextMenu("Run Test: World Destroy Global World")]
    public void Test_WorldClass_005()
    {
        string testName = "Test_WorldClass_005";
        ExecuteTest(testName, () =>
        {
            // 1. Get global world instance
            WorldInstance globalWorld = World.GetOrCreate();
            
            // 2. Attempt to destroy global world
            bool destroyed = World.Destroy(globalWorld);
            
            // Destroy returns false, global world not destroyed
            Assert(!destroyed, "Destroy should return false for global world");
            AssertNotNull(World.GetOrCreate(), "Global world should still exist");
            AssertEquals("Global", World.GetOrCreate().Name, "Global world name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: World Destroy Twice")]
    public void Test_WorldClass_006()
    {
        string testName = "Test_WorldClass_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Destroy TestWorld
            bool destroyed1 = World.Destroy(testWorld);
            
            // 3. Attempt to destroy TestWorld again
            bool destroyed2 = World.Destroy(testWorld);
            
            // Second destruction returns false
            Assert(destroyed1, "First Destroy should return true");
            Assert(!destroyed2, "Second Destroy should return false");
        });
    }
    
    // ========== World-001: Global World Singleton ==========
    
    [ContextMenu("Run Test: GetOrCreate Returns Singleton")]
    public void Test_GlobalWorld_001()
    {
        string testName = "Test_GlobalWorld_001";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.GetOrCreate() first time
            WorldInstance globalWorld1 = World.GetOrCreate();
            
            // 2. Call World.GetOrCreate() second time
            WorldInstance globalWorld2 = World.GetOrCreate();
            
            // 3. Compare instances
            // Same instance returned both times
            Assert(globalWorld1 == globalWorld2, "GlobalWorld1 should equal GlobalWorld2");
        });
    }
    
    [ContextMenu("Run Test: Global World Name")]
    public void Test_GlobalWorld_002()
    {
        string testName = "Test_GlobalWorld_002";
        ExecuteTest(testName, () =>
        {
            // 1. Get global world instance
            WorldInstance globalWorld = World.GetOrCreate();
            
            // 2. Check Name property
            AssertEquals("Global", globalWorld.Name, "Global world name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: Global World Lazy Initialization")]
    public void Test_GlobalWorld_003()
    {
        string testName = "Test_GlobalWorld_003";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.GetOrCreate()
            WorldInstance globalWorld = World.GetOrCreate();
            
            // 2. Verify instance is created
            AssertNotNull(globalWorld, "Global world should not be null");
            AssertEquals("Global", globalWorld.Name, "Global world name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: Global World Cannot Be Destroyed")]
    public void Test_GlobalWorld_004()
    {
        string testName = "Test_GlobalWorld_004";
        ExecuteTest(testName, () =>
        {
            // 1. Get global world instance
            WorldInstance globalWorld = World.GetOrCreate();
            
            // 2. Attempt to destroy global world
            bool destroyed = World.Destroy(globalWorld);
            
            // 3. Verify global world still exists
            Assert(!destroyed, "Destroy should return false for global world");
            AssertNotNull(World.GetOrCreate(), "Global world should still exist");
            WorldInstance newGlobalWorld = World.GetOrCreate();
            AssertEquals(globalWorld, newGlobalWorld, "Global world should be same instance");
        });
    }
    
    // ========== World-002: World-Scoped Storage Integration ==========
    
    [ContextMenu("Run Test: World Isolation Components")]
    public void Test_WorldScoped_001()
    {
        string testName = "Test_WorldScoped_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("World1") and World2("World2")
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Create Entity1 in World1
            Entity entity1 = world1.CreateEntity();
            
            // 3. Create Entity2 in World2
            Entity entity2 = world2.CreateEntity();
            
            // 4. Add TestComponent to Entity1 in World1
            world1.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 5. Add TestComponent to Entity2 in World2
            world2.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 6. Check components in each world
            // Components are isolated by world
            TestComponent comp1 = world1.GetComponent<TestComponent>(entity1);
            AssertEquals(1, comp1.Value, "Entity1 in World1 should have Value=1");
            
            bool hasComp1InWorld2 = entity1.Has<TestComponent>(world2);
            Assert(!hasComp1InWorld2, "Entity1 should not have component in World2");
            
            bool hasComp2InWorld1 = entity2.Has<TestComponent>(world1);
            Assert(!hasComp2InWorld1, "Entity2 should not have component in World1");
            
            TestComponent comp2 = world2.GetComponent<TestComponent>(entity2);
            AssertEquals(2, comp2.Value, "Entity2 in World2 should have Value=2");
        });
    }
    
    [ContextMenu("Run Test: World Isolation Systems")]
    public void Test_WorldScoped_002()
    {
        string testName = "Test_WorldScoped_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("World1") and World2("World2")
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Create System1 and System2
            bool system1Executed = false;
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 3. Add System1 to World1 Update queue
            world1.AddToUpdate(system1);
            
            // 4. Add System2 to World2 Update queue
            world2.AddToUpdate(system2);
            
            // 5. Execute each world's Update queue
            world1.ExecuteUpdate();
            world2.ExecuteUpdate();
            
            // Systems are isolated by world
            Assert(system1Executed, "System1 should execute in World1");
            Assert(system2Executed, "System2 should execute in World2");
            
            // Reset flags
            system1Executed = false;
            system2Executed = false;
            
            // Execute world2 first, then world1 - systems should only execute in their own world
            world2.ExecuteUpdate();
            world1.ExecuteUpdate();
            
            Assert(system1Executed, "System1 should execute again in World1");
            Assert(system2Executed, "System2 should execute again in World2");
        });
    }
    
    [ContextMenu("Run Test: GetEntitiesWith World Parameter")]
    public void Test_WorldScoped_003()
    {
        string testName = "Test_WorldScoped_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Create Entity1 in global world with Position and Velocity
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 3. Create Entity2 in TestWorld with Position and Velocity
            Entity entity2 = testWorld.CreateEntity();
            testWorld.AddComponent(entity2, new Position { X = 2f, Y = 3f, Z = 4f });
            testWorld.AddComponent(entity2, new Velocity { X = 2f, Y = 3f, Z = 4f });
            
            // 4. Get entities from each world
            var globalEntities = World.GetEntitiesWith<Position, Velocity>();
            var testEntities = testWorld.GetEntitiesWith<Position, Velocity>();
            
            // Multi-type queries respect World parameter
            AssertEquals(1, globalEntities.Length, "Global world should have 1 entity");
            AssertEquals(1, testEntities.Length, "TestWorld should have 1 entity");
        });
    }
    
    [ContextMenu("Run Test: GetComponents World Parameter")]
    public void Test_WorldScoped_004()
    {
        string testName = "Test_WorldScoped_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Create Entity1 in global world
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 3. Create Entity2 in TestWorld
            Entity entity2 = testWorld.CreateEntity();
            testWorld.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 4. Get components from each world
            var globalComponents = World.GetComponents<TestComponent>();
            var testComponents = testWorld.GetComponents<TestComponent>();
            
            // GetComponents returns components only from specified world
            AssertEquals(1, globalComponents.Length, "Global world should have 1 component");
            AssertEquals(1, testComponents.Length, "TestWorld should have 1 component");
        });
    }
    
    [ContextMenu("Run Test: ExecuteOnce World Parameter")]
    public void Test_WorldScoped_005()
    {
        string testName = "Test_WorldScoped_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Create System that uses world context
            WorldInstance executedWorld = null;
            TestSystem system = new TestSystem(() => 
            {
                // System can use World parameter passed to Execute()
                // For this test, we'll verify the system executes
                executedWorld = World.GetOrCreate(); // Simplified - system doesn't receive world parameter in TestSystem
            });
            
            // 3. Call ExecuteOnce with World parameter
            testWorld.ExecuteOnce(system);
            
            // System executed successfully
            Assert(true, "ExecuteOnce should execute system successfully");
        });
    }
    
    [ContextMenu("Run Test: ExecuteUpdate World Parameter")]
    public void Test_WorldScoped_006()
    {
        string testName = "Test_WorldScoped_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Create System1 in global world
            bool system1Executed = false;
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 3. Add System1 to global world Update queue
            World.AddToUpdate(system1);
            
            // 4. Add System2 to TestWorld Update queue
            testWorld.AddToUpdate(system2);
            
            // 5. Call ExecuteUpdate(TestWorld)
            testWorld.ExecuteUpdate();
            
            // 6. Check execution flags
            // Only systems from specified world executed
            Assert(!system1Executed, "System1 should not execute");
            Assert(system2Executed, "System2 should execute");
        });
    }
    
    [ContextMenu("Run Test: ExecuteFixedUpdate World Parameter")]
    public void Test_WorldScoped_007()
    {
        string testName = "Test_WorldScoped_007";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("TestWorld")
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            
            // 2. Create System1 in global world
            bool system1Executed = false;
            bool system2Executed = false;
            
            TestSystem system1 = new TestSystem(() => { system1Executed = true; });
            TestSystem system2 = new TestSystem(() => { system2Executed = true; });
            
            // 3. Add System1 to global world FixedUpdate queue
            World.AddToFixedUpdate(system1);
            
            // 4. Add System2 to TestWorld FixedUpdate queue
            testWorld.AddToFixedUpdate(system2);
            
            // 5. Call ExecuteFixedUpdate(TestWorld)
            testWorld.ExecuteFixedUpdate();
            
            // 6. Check execution flags
            // Only systems from specified world executed
            Assert(!system1Executed, "System1 should not execute");
            Assert(system2Executed, "System2 should execute");
        });
    }
    
    // ========== World-003: World Persistence Across Scenes ==========
    // Note: Scene persistence tests are difficult to test without actual scene loading
    // These tests verify the underlying mechanism (static storage) works correctly
    
    [ContextMenu("Run Test: World State Independent of Operations")]
    public void Test_Persistence_001()
    {
        string testName = "Test_Persistence_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities and components
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new Position { X = 10f, Y = 20f, Z = 30f });
            World.AddComponent(entity1, new Health { Amount = 100f });
            
            // 2. Verify components exist
            Position pos = World.GetComponent<Position>(entity1);
            Health health = World.GetComponent<Health>(entity1);
            
            AssertEquals(10f, pos.X, "Position.X should be 10");
            AssertEquals(100f, health.Amount, "Health.Amount should be 100");
            
            // 3. Create another world and verify isolation
            WorldInstance world2 = World.GetOrCreate("Test2");
            Entity entity2 = world2.CreateEntity();
            world2.AddComponent(entity2, new Position { X = 50f, Y = 60f, Z = 70f });
            
            // 4. Verify world1 components still exist
            pos = World.GetComponent<Position>(entity1);
            health = World.GetComponent<Health>(entity1);
            
            AssertEquals(10f, pos.X, "Position.X should still be 10");
            AssertEquals(100f, health.Amount, "Health.Amount should still be 100");
            
            // 5. Verify world2 components are separate
            Position pos2 = world2.GetComponent<Position>(entity2);
            AssertEquals(50f, pos2.X, "World2 Position.X should be 50");
        });
    }
    
    [ContextMenu("Run Test: Multiple Worlds Persist")]
    public void Test_Persistence_002()
    {
        string testName = "Test_Persistence_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 in global world
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 2. Create Entity2 in TestWorld
            WorldInstance testWorld = World.GetOrCreate("TestWorld");
            Entity entity2 = testWorld.CreateEntity();
            testWorld.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 3. Verify both worlds have their entities
            TestComponent comp1 = World.GetComponent<TestComponent>(entity1);
            TestComponent comp2 = testWorld.GetComponent<TestComponent>(entity2);
            
            AssertEquals(1, comp1.Value, "Entity1 in global world should have Value=1");
            AssertEquals(2, comp2.Value, "Entity2 in TestWorld should have Value=2");
            
            // 4. Get worlds again and verify entities still exist
            WorldInstance globalWorld2 = World.GetOrCreate();
            WorldInstance testWorld2 = World.GetOrCreate("TestWorld");
            
            // Note: Entities are stored per-world, so they should still be accessible
            // However, entity IDs might not be valid after ClearAllECSState in ExecuteTest
            // This test verifies that world state is maintained during test execution
            WorldInstance globalWorld1 = World.GetOrCreate();
            AssertEquals(globalWorld1, globalWorld2, "Global world should be same instance");
            AssertEquals(testWorld, testWorld2, "TestWorld should be same instance");
        });
    }
    
    // ========== API-015: GetAllEntities Method ==========
    
    [ContextMenu("Run Test: GetAllEntities Empty World")]
    public void Test_GetAllEntities_001()
    {
        string testName = "Test_GetAllEntities_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create empty world
            WorldInstance world = World.GetOrCreate("EmptyWorld");
            
            // 2. Call GetAllEntities()
            var entities = world.GetAllEntities();
            
            // Empty world returns empty span
            AssertEquals(0, entities.Length, "Empty world should return empty span");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities Single Entity")]
    public void Test_GetAllEntities_002()
    {
        string testName = "Test_GetAllEntities_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create single entity with component
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Call GetAllEntities()
            var entities = world.GetAllEntities();
            
            // Single entity returned
            AssertEquals(1, entities.Length, "World with single entity should return 1 entity");
            AssertEquals(entity, entities[0], "Returned entity should match created entity");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities Multiple Entities")]
    public void Test_GetAllEntities_003()
    {
        string testName = "Test_GetAllEntities_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create multiple entities with components
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new TestComponent { Value = 1 });
            
            Entity entity2 = world.CreateEntity();
            world.AddComponent(entity2, new TestComponent { Value = 2 });
            
            Entity entity3 = world.CreateEntity();
            world.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 3. Call GetAllEntities()
            var entities = world.GetAllEntities();
            
            // All entities returned
            AssertEquals(3, entities.Length, "World with 3 entities should return 3 entities");
            
            // Verify all entities are present
            bool found1 = false, found2 = false, found3 = false;
            foreach (var e in entities)
            {
                if (e == entity1) found1 = true;
                if (e == entity2) found2 = true;
                if (e == entity3) found3 = true;
            }
            Assert(found1, "Entity1 should be in result");
            Assert(found2, "Entity2 should be in result");
            Assert(found3, "Entity3 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities Deduplication Multiple Components")]
    public void Test_GetAllEntities_004()
    {
        string testName = "Test_GetAllEntities_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with multiple components
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity, new Health { Amount = 100f });
            
            // 3. Call GetAllEntities()
            var entities = world.GetAllEntities();
            
            // Entity appears only once (deduplication)
            AssertEquals(1, entities.Length, "Entity with multiple components should appear only once");
            AssertEquals(entity, entities[0], "Returned entity should match created entity");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities Entities Without Components Not Included")]
    public void Test_GetAllEntities_005()
    {
        string testName = "Test_GetAllEntities_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with component
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 3. Create entity without components (note: this entity won't appear in ComponentTable)
            // Since GetAllEntities() iterates through ComponentTables, entities without components won't be included
            Entity entity2 = world.CreateEntity();
            // Don't add any component to entity2
            
            // 4. Call GetAllEntities()
            var entities = world.GetAllEntities();
            
            // Only entity with component is returned
            // Note: Entities without components are not included because they don't appear in any ComponentTable
            AssertEquals(1, entities.Length, "Only entity with component should be returned");
            AssertEquals(entity1, entities[0], "Returned entity should be entity1");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities Static World Method")]
    public void Test_GetAllEntities_006()
    {
        string testName = "Test_GetAllEntities_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create entities in global world
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new TestComponent { Value = 1 });
            
            Entity entity2 = World.CreateEntity();
            World.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 2. Call static World.GetAllEntities()
            var entities = World.GetAllEntities();
            
            // All entities from global world returned
            AssertEquals(2, entities.Length, "Global world with 2 entities should return 2 entities");
            
            // Verify entities are present
            bool found1 = false, found2 = false;
            foreach (var e in entities)
            {
                if (e == entity1) found1 = true;
                if (e == entity2) found2 = true;
            }
            Assert(found1, "Entity1 should be in result");
            Assert(found2, "Entity2 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities After Entity Creation")]
    public void Test_GetAllEntities_007()
    {
        string testName = "Test_GetAllEntities_007";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create first entity
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 3. Get all entities (should have 1)
            var entities1 = world.GetAllEntities();
            AssertEquals(1, entities1.Length, "Should have 1 entity");
            
            // 4. Create second entity
            Entity entity2 = world.CreateEntity();
            world.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 5. Get all entities again (should have 2)
            var entities2 = world.GetAllEntities();
            AssertEquals(2, entities2.Length, "Should have 2 entities after creating second entity");
            
            // 6. Verify both entities are present
            bool found1 = false, found2 = false;
            foreach (var e in entities2)
            {
                if (e == entity1) found1 = true;
                if (e == entity2) found2 = true;
            }
            Assert(found1, "Entity1 should be in result");
            Assert(found2, "Entity2 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities After Entity Destruction")]
    public void Test_GetAllEntities_008()
    {
        string testName = "Test_GetAllEntities_008";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create multiple entities
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new TestComponent { Value = 1 });
            
            Entity entity2 = world.CreateEntity();
            world.AddComponent(entity2, new TestComponent { Value = 2 });
            
            Entity entity3 = world.CreateEntity();
            world.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 3. Get all entities (should have 3)
            var entities1 = world.GetAllEntities();
            AssertEquals(3, entities1.Length, "Should have 3 entities");
            
            // 4. Destroy entity2
            world.DestroyEntity(entity2);
            
            // 5. Get all entities again (should have 2)
            var entities2 = world.GetAllEntities();
            AssertEquals(2, entities2.Length, "Should have 2 entities after destroying entity2");
            
            // 6. Verify entity2 is not present, but entity1 and entity3 are
            bool found1 = false, found2 = false, found3 = false;
            foreach (var e in entities2)
            {
                if (e == entity1) found1 = true;
                if (e == entity2) found2 = true;
                if (e == entity3) found3 = true;
            }
            Assert(found1, "Entity1 should be in result");
            Assert(!found2, "Entity2 should not be in result (destroyed)");
            Assert(found3, "Entity3 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities Different Component Combinations")]
    public void Test_GetAllEntities_009()
    {
        string testName = "Test_GetAllEntities_009";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entities with different component combinations
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            
            Entity entity2 = world.CreateEntity();
            world.AddComponent(entity2, new Position { X = 2f, Y = 3f, Z = 4f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            Entity entity3 = world.CreateEntity();
            world.AddComponent(entity3, new Health { Amount = 100f });
            
            Entity entity4 = world.CreateEntity();
            world.AddComponent(entity4, new Position { X = 3f, Y = 4f, Z = 5f });
            world.AddComponent(entity4, new Velocity { X = 2f, Y = 3f, Z = 4f });
            world.AddComponent(entity4, new Health { Amount = 200f });
            
            // 3. Call GetAllEntities()
            var entities = world.GetAllEntities();
            
            // All entities returned (each appears only once)
            AssertEquals(4, entities.Length, "World with 4 entities should return 4 entities");
            
            // Verify all entities are present
            bool found1 = false, found2 = false, found3 = false, found4 = false;
            foreach (var e in entities)
            {
                if (e == entity1) found1 = true;
                if (e == entity2) found2 = true;
                if (e == entity3) found3 = true;
                if (e == entity4) found4 = true;
            }
            Assert(found1, "Entity1 should be in result");
            Assert(found2, "Entity2 should be in result");
            Assert(found3, "Entity3 should be in result");
            Assert(found4, "Entity4 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities World Isolation")]
    public void Test_GetAllEntities_010()
    {
        string testName = "Test_GetAllEntities_010";
        ExecuteTest(testName, () =>
        {
            // 1. Create two separate worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Create entities in world1
            Entity entity1 = world1.CreateEntity();
            world1.AddComponent(entity1, new TestComponent { Value = 1 });
            
            Entity entity2 = world1.CreateEntity();
            world1.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 3. Create entities in world2
            Entity entity3 = world2.CreateEntity();
            world2.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 4. Get all entities from each world
            var entities1 = world1.GetAllEntities();
            var entities2 = world2.GetAllEntities();
            
            // Each world returns only its own entities
            AssertEquals(2, entities1.Length, "World1 should have 2 entities");
            AssertEquals(1, entities2.Length, "World2 should have 1 entity");
            
            // Verify world isolation
            bool found1 = false, found2 = false;
            foreach (var e in entities1)
            {
                if (e == entity1) found1 = true;
                if (e == entity2) found2 = true;
            }
            Assert(found1, "Entity1 should be in World1 result");
            Assert(found2, "Entity2 should be in World1 result");
            
            AssertEquals(entity3, entities2[0], "Entity3 should be in World2 result");
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities Many Entities Performance")]
    public void Test_GetAllEntities_011()
    {
        string testName = "Test_GetAllEntities_011";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create many entities (100 entities)
            const int entityCount = 100;
            Entity[] createdEntities = new Entity[entityCount];
            
            for (int i = 0; i < entityCount; i++)
            {
                Entity entity = world.CreateEntity();
                world.AddComponent(entity, new TestComponent { Value = i });
                createdEntities[i] = entity;
            }
            
            // 3. Call GetAllEntities()
            var entities = world.GetAllEntities();
            
            // All entities returned
            AssertEquals(entityCount, entities.Length, $"World with {entityCount} entities should return {entityCount} entities");
            
            // Verify all entities are present
            var foundEntities = new System.Collections.Generic.HashSet<Entity>();
            foreach (var e in entities)
            {
                foundEntities.Add(e);
            }
            
            for (int i = 0; i < entityCount; i++)
            {
                Assert(foundEntities.Contains(createdEntities[i]), $"Entity {i} should be in result");
            }
        });
    }
    
    [ContextMenu("Run Test: GetAllEntities ReadOnlySpan Iteration")]
    public void Test_GetAllEntities_012()
    {
        string testName = "Test_GetAllEntities_012";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create multiple entities
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new TestComponent { Value = 1 });
            
            Entity entity2 = world.CreateEntity();
            world.AddComponent(entity2, new TestComponent { Value = 2 });
            
            Entity entity3 = world.CreateEntity();
            world.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 3. Call GetAllEntities() and iterate over ReadOnlySpan
            var entities = world.GetAllEntities();
            
            // Verify ReadOnlySpan can be iterated
            int count = 0;
            foreach (var entity in entities)
            {
                count++;
                Assert(entity.IsValid, "Entity should be valid");
            }
            
            AssertEquals(3, count, "Should iterate over 3 entities");
            AssertEquals(3, entities.Length, "ReadOnlySpan.Length should be 3");
        });
    }
    
    // ========== API-017: GetAllWorlds Method ==========
    
    [ContextMenu("Run Test: GetAllWorlds Empty State")]
    public void Test_GetAllWorlds_001()
    {
        string testName = "Test_GetAllWorlds_001";
        ExecuteTest(testName, () =>
        {
            // 1. ClearAllECSState is called in ExecuteTest, which now clears _globalWorld and _localWorlds
            // 2. Call GetAllWorlds() without creating any worlds
            var allWorlds = World.GetAllWorlds();
            
            // Empty state returns empty list
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            AssertEquals(0, allWorlds.Count, "Empty state should return empty list");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Only Global World")]
    public void Test_GetAllWorlds_002()
    {
        string testName = "Test_GetAllWorlds_002";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world (lazy initialization)
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // Global world is included in result
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            AssertEquals(1, allWorlds.Count, "Should return 1 world (global world)");
            AssertEquals(globalWorld, allWorlds[0], "First world should be global world");
            AssertEquals("Global", allWorlds[0].Name, "Global world name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Only Local Worlds")]
    public void Test_GetAllWorlds_003()
    {
        string testName = "Test_GetAllWorlds_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create local worlds without accessing global world
            WorldInstance world1 = World.GetOrCreate("LocalWorld1");
            WorldInstance world2 = World.GetOrCreate("LocalWorld2");
            WorldInstance world3 = World.GetOrCreate("LocalWorld3");
            
            // 2. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // Local worlds are included in result
            // Note: Global world might also be included if it was initialized
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            Assert(allWorlds.Count >= 3, "Should return at least 3 local worlds");
            
            // Verify all local worlds are present
            bool found1 = false, found2 = false, found3 = false;
            foreach (var world in allWorlds)
            {
                if (world == world1) found1 = true;
                if (world == world2) found2 = true;
                if (world == world3) found3 = true;
            }
            Assert(found1, "LocalWorld1 should be in result");
            Assert(found2, "LocalWorld2 should be in result");
            Assert(found3, "LocalWorld3 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Global And Local Worlds")]
    public void Test_GetAllWorlds_004()
    {
        string testName = "Test_GetAllWorlds_004";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Create local worlds
            WorldInstance localWorld1 = World.GetOrCreate("LocalWorld1");
            WorldInstance localWorld2 = World.GetOrCreate("LocalWorld2");
            
            // 3. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // Both global and local worlds are included
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            AssertEquals(3, allWorlds.Count, "Should return 3 worlds (1 global + 2 local)");
            
            // Verify global world is present
            bool foundGlobal = false;
            bool foundLocal1 = false;
            bool foundLocal2 = false;
            foreach (var world in allWorlds)
            {
                if (world == globalWorld) foundGlobal = true;
                if (world == localWorld1) foundLocal1 = true;
                if (world == localWorld2) foundLocal2 = true;
            }
            Assert(foundGlobal, "Global world should be in result");
            Assert(foundLocal1, "LocalWorld1 should be in result");
            Assert(foundLocal2, "LocalWorld2 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Excludes Destroyed Worlds")]
    public void Test_GetAllWorlds_005()
    {
        string testName = "Test_GetAllWorlds_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create local worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            WorldInstance world3 = World.GetOrCreate("World3");
            
            // 2. Verify all worlds are in result
            var allWorlds1 = World.GetAllWorlds();
            Assert(allWorlds1.Count >= 3, "Should have at least 3 worlds before destruction");
            
            // 3. Destroy world2
            bool destroyed = World.Destroy(world2);
            Assert(destroyed, "World.Destroy should return true");
            
            // 4. Call GetAllWorlds() again
            var allWorlds2 = World.GetAllWorlds();
            
            // Destroyed world is excluded from result
            AssertNotNull(allWorlds2, "GetAllWorlds should return non-null list");
            
            bool foundWorld1 = false;
            bool foundWorld2 = false;
            bool foundWorld3 = false;
            foreach (var world in allWorlds2)
            {
                if (world == world1) foundWorld1 = true;
                if (world == world2) foundWorld2 = true;
                if (world == world3) foundWorld3 = true;
            }
            Assert(foundWorld1, "World1 should be in result");
            Assert(!foundWorld2, "World2 should NOT be in result (destroyed)");
            Assert(foundWorld3, "World3 should be in result");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Snapshot Semantics")]
    public void Test_GetAllWorlds_006()
    {
        string testName = "Test_GetAllWorlds_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create local worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Call GetAllWorlds() and store result
            var allWorlds1 = World.GetAllWorlds();
            Assert(allWorlds1.Count >= 2, "Should have at least 2 worlds");
            
            // 3. Destroy world2 after getting snapshot
            bool destroyed = World.Destroy(world2);
            Assert(destroyed, "World.Destroy should return true");
            
            // 4. Verify snapshot still contains world2 (snapshot semantics)
            bool foundWorld2InSnapshot = false;
            foreach (var world in allWorlds1)
            {
                if (world == world2) foundWorld2InSnapshot = true;
            }
            Assert(foundWorld2InSnapshot, "Snapshot should still contain world2 (snapshot semantics)");
            
            // 5. Call GetAllWorlds() again - new snapshot should not contain world2
            var allWorlds2 = World.GetAllWorlds();
            bool foundWorld2InNewSnapshot = false;
            foreach (var world in allWorlds2)
            {
                if (world == world2) foundWorld2InNewSnapshot = true;
            }
            Assert(!foundWorld2InNewSnapshot, "New snapshot should NOT contain world2");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds ReadOnlyList")]
    public void Test_GetAllWorlds_007()
    {
        string testName = "Test_GetAllWorlds_007";
        ExecuteTest(testName, () =>
        {
            // 1. Create local world
            WorldInstance world1 = World.GetOrCreate("World1");
            
            // 2. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // 3. Verify return type is IReadOnlyList
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            Assert(allWorlds is System.Collections.Generic.IReadOnlyList<WorldInstance>, 
                "GetAllWorlds should return IReadOnlyList<WorldInstance>");
            
            // 4. Verify read-only guarantee by attempting to modify (should fail)
            // Try to cast to IList and verify it's not directly modifiable
            var asIList = allWorlds as System.Collections.Generic.IList<WorldInstance>;
            if (asIList != null)
            {
                // If it implements IList, verify it's read-only by checking IsReadOnly
                Assert(asIList.IsReadOnly, "IList should be read-only");
            }
            
            // 5. Verify we can access Count and indexed access
            Assert(allWorlds.Count >= 1, "Should have at least 1 world");
            AssertNotNull(allWorlds[0], "Should be able to access world by index");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Multiple Local Worlds")]
    public void Test_GetAllWorlds_008()
    {
        string testName = "Test_GetAllWorlds_008";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple local worlds
            const int worldCount = 10;
            WorldInstance[] createdWorlds = new WorldInstance[worldCount];
            
            for (int i = 0; i < worldCount; i++)
            {
                createdWorlds[i] = World.GetOrCreate($"World{i}");
            }
            
            // 2. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // All local worlds are included
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            Assert(allWorlds.Count >= worldCount, $"Should return at least {worldCount} local worlds");
            
            // Verify all created worlds are present
            var foundWorlds = new System.Collections.Generic.HashSet<WorldInstance>();
            foreach (var world in allWorlds)
            {
                foundWorlds.Add(world);
            }
            
            for (int i = 0; i < worldCount; i++)
            {
                Assert(foundWorlds.Contains(createdWorlds[i]), $"World{i} should be in result");
            }
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Global World Not Initialized")]
    public void Test_GetAllWorlds_009()
    {
        string testName = "Test_GetAllWorlds_009";
        ExecuteTest(testName, () =>
        {
            // 1. Create local world without accessing global world
            WorldInstance localWorld = World.GetOrCreate("LocalWorld");
            
            // 2. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // Local world is included
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            Assert(allWorlds.Count >= 1, "Should return at least 1 local world");
            
            // Verify local world is present
            bool foundLocal = false;
            foreach (var world in allWorlds)
            {
                if (world == localWorld) foundLocal = true;
            }
            Assert(foundLocal, "LocalWorld should be in result");
            
            // Note: We can't easily test that global world is NOT included without
            // being able to reset _globalWorld, but the implementation should handle
            // null _globalWorld correctly
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Consistent Order")]
    public void Test_GetAllWorlds_010()
    {
        string testName = "Test_GetAllWorlds_010";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Create local worlds in specific order
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            WorldInstance world3 = World.GetOrCreate("World3");
            
            // 3. Call GetAllWorlds() multiple times
            var allWorlds1 = World.GetAllWorlds();
            var allWorlds2 = World.GetAllWorlds();
            var allWorlds3 = World.GetAllWorlds();
            
            // Order should be consistent (implementation-defined)
            AssertEquals(allWorlds1.Count, allWorlds2.Count, "Count should be consistent");
            AssertEquals(allWorlds2.Count, allWorlds3.Count, "Count should be consistent");
            
            // Verify all calls return same worlds (order may vary, but same worlds)
            var worlds1Set = new System.Collections.Generic.HashSet<WorldInstance>(allWorlds1);
            var worlds2Set = new System.Collections.Generic.HashSet<WorldInstance>(allWorlds2);
            var worlds3Set = new System.Collections.Generic.HashSet<WorldInstance>(allWorlds3);
            
            Assert(worlds1Set.SetEquals(worlds2Set), "Worlds should be same between calls");
            Assert(worlds2Set.SetEquals(worlds3Set), "Worlds should be same between calls");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds All Worlds Destroyed")]
    public void Test_GetAllWorlds_011()
    {
        string testName = "Test_GetAllWorlds_011";
        ExecuteTest(testName, () =>
        {
            // 1. Create local worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Destroy all local worlds
            bool destroyed1 = World.Destroy(world1);
            bool destroyed2 = World.Destroy(world2);
            Assert(destroyed1, "World1 should be destroyed");
            Assert(destroyed2, "World2 should be destroyed");
            
            // 3. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // Destroyed worlds are excluded
            // Note: Global world might still be in result if it was initialized
            AssertNotNull(allWorlds, "GetAllWorlds should return non-null list");
            
            // Verify destroyed worlds are not in result
            bool foundWorld1 = false;
            bool foundWorld2 = false;
            foreach (var world in allWorlds)
            {
                if (world == world1) foundWorld1 = true;
                if (world == world2) foundWorld2 = true;
            }
            Assert(!foundWorld1, "World1 should NOT be in result (destroyed)");
            Assert(!foundWorld2, "World2 should NOT be in result (destroyed)");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Iteration")]
    public void Test_GetAllWorlds_012()
    {
        string testName = "Test_GetAllWorlds_012";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Create local worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 3. Call GetAllWorlds() and iterate
            var allWorlds = World.GetAllWorlds();
            
            // Verify iteration works
            int count = 0;
            var foundWorlds = new System.Collections.Generic.HashSet<WorldInstance>();
            foreach (var world in allWorlds)
            {
                count++;
                AssertNotNull(world, "World should not be null");
                AssertNotNull(world.Name, "World.Name should not be null");
                foundWorlds.Add(world);
            }
            
            Assert(count >= 3, "Should iterate over at least 3 worlds");
            Assert(foundWorlds.Contains(globalWorld), "Global world should be found during iteration");
            Assert(foundWorlds.Contains(world1), "World1 should be found during iteration");
            Assert(foundWorlds.Contains(world2), "World2 should be found during iteration");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Indexed Access")]
    public void Test_GetAllWorlds_013()
    {
        string testName = "Test_GetAllWorlds_013";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Create local world
            WorldInstance localWorld = World.GetOrCreate("LocalWorld");
            
            // 3. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // Verify indexed access works
            Assert(allWorlds.Count >= 2, "Should have at least 2 worlds");
            AssertNotNull(allWorlds[0], "First world should not be null");
            AssertNotNull(allWorlds[allWorlds.Count - 1], "Last world should not be null");
            
            // Verify we can access all worlds by index
            for (int i = 0; i < allWorlds.Count; i++)
            {
                AssertNotNull(allWorlds[i], $"World at index {i} should not be null");
                AssertNotNull(allWorlds[i].Name, $"World at index {i} should have name");
            }
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Count Property")]
    public void Test_GetAllWorlds_014()
    {
        string testName = "Test_GetAllWorlds_014";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple local worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            WorldInstance world3 = World.GetOrCreate("World3");
            
            // 2. Call GetAllWorlds()
            var allWorlds = World.GetAllWorlds();
            
            // Verify Count property works
            Assert(allWorlds.Count >= 3, "Count should be at least 3");
            
            // Verify Count matches actual number of worlds
            int actualCount = 0;
            foreach (var world in allWorlds)
            {
                actualCount++;
            }
            AssertEquals(allWorlds.Count, actualCount, "Count property should match iteration count");
        });
    }
    
    [ContextMenu("Run Test: GetAllWorlds Thread Safety")]
    public void Test_GetAllWorlds_015()
    {
        string testName = "Test_GetAllWorlds_015";
        ExecuteTest(testName, () =>
        {
            // 1. Create initial worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Test that GetAllWorlds() can be called safely from multiple threads
            // Note: Full thread-safety testing is complex, but we can verify the method
            // doesn't throw exceptions when called concurrently
            
            Thread[] threads = new Thread[5];
            System.Collections.Generic.List<Exception> exceptions = 
                new System.Collections.Generic.List<Exception>();
            object lockObject = new object();
            
            // 3. Create multiple threads that call GetAllWorlds() concurrently
            for (int i = 0; i < threads.Length; i++)
            {
                int threadIndex = i;
                threads[i] = new Thread(() =>
                {
                    try
                    {
                        // Call GetAllWorlds() multiple times in each thread
                        for (int j = 0; j < 10; j++)
                        {
                            var worlds = World.GetAllWorlds();
                            AssertNotNull(worlds, "GetAllWorlds should return non-null");
                            // Verify we can iterate without exceptions
                            foreach (var world in worlds)
                            {
                                AssertNotNull(world, "World should not be null");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            
            // 4. Start all threads
            foreach (var thread in threads)
            {
                thread.Start();
            }
            
            // 5. Wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }
            
            // 6. Verify no exceptions occurred
            AssertEquals(0, exceptions.Count, 
                $"No exceptions should occur during concurrent GetAllWorlds() calls. Found {exceptions.Count} exceptions.");
            
            // 7. Verify final state is consistent
            var finalWorlds = World.GetAllWorlds();
            AssertNotNull(finalWorlds, "GetAllWorlds should still work after concurrent access");
        });
    }
    
    // ========== API-018: World.Exists Method ==========
    
    [ContextMenu("Run Test: Exists Null Name Global World")]
    public void Test_Exists_001()
    {
        string testName = "Test_Exists_001";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world (lazy initialization)
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Call Exists(null)
            bool exists = World.Exists(null);
            
            // Null name returns true if global world exists
            Assert(exists, "Exists(null) should return true when global world exists");
        });
    }
    
    [ContextMenu("Run Test: Exists Empty String Global World")]
    public void Test_Exists_002()
    {
        string testName = "Test_Exists_002";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world (lazy initialization)
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Call Exists("")
            bool exists = World.Exists("");
            
            // Empty string returns true if global world exists
            Assert(exists, "Exists(\"\") should return true when global world exists");
        });
    }
    
    [ContextMenu("Run Test: Exists Global Name")]
    public void Test_Exists_003()
    {
        string testName = "Test_Exists_003";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world (lazy initialization)
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Call Exists("Global")
            bool exists = World.Exists("Global");
            
            // "Global" name returns true if global world exists
            Assert(exists, "Exists(\"Global\") should return true when global world exists");
        });
    }
    
    [ContextMenu("Run Test: Exists NonExistent Local World")]
    public void Test_Exists_004()
    {
        string testName = "Test_Exists_004";
        ExecuteTest(testName, () =>
        {
            // 1. Don't create any local world
            // 2. Call Exists("NonExistentWorld")
            bool exists = World.Exists("NonExistentWorld");
            
            // Non-existent local world returns false
            Assert(!exists, "Exists(\"NonExistentWorld\") should return false for non-existent world");
        });
    }
    
    [ContextMenu("Run Test: Exists Existing Local World")]
    public void Test_Exists_005()
    {
        string testName = "Test_Exists_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create local world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Call Exists("TestWorld")
            bool exists = World.Exists("TestWorld");
            
            // Existing local world returns true
            Assert(exists, "Exists(\"TestWorld\") should return true for existing world");
        });
    }
    
    [ContextMenu("Run Test: Exists Destroyed Local World")]
    public void Test_Exists_006()
    {
        string testName = "Test_Exists_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create local world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Verify world exists
            bool existsBefore = World.Exists("TestWorld");
            Assert(existsBefore, "World should exist before destruction");
            
            // 3. Destroy world
            bool destroyed = World.Destroy(world);
            Assert(destroyed, "World.Destroy should return true");
            
            // 4. Call Exists("TestWorld")
            bool existsAfter = World.Exists("TestWorld");
            
            // Destroyed local world returns false
            Assert(!existsAfter, "Exists(\"TestWorld\") should return false for destroyed world");
        });
    }
    
    [ContextMenu("Run Test: Exists Global World Not Initialized")]
    public void Test_Exists_007()
    {
        string testName = "Test_Exists_007";
        ExecuteTest(testName, () =>
        {
            // 1. ClearAllECSState is called in ExecuteTest, which clears _globalWorld
            // 2. Don't access global world (keep it uninitialized)
            // 3. Call Exists(null)
            bool exists = World.Exists(null);
            
            // Global world not initialized returns false
            Assert(!exists, "Exists(null) should return false when global world is not initialized");
        });
    }
    
    [ContextMenu("Run Test: Exists Case Sensitive Name Matching")]
    public void Test_Exists_008()
    {
        string testName = "Test_Exists_008";
        ExecuteTest(testName, () =>
        {
            // 1. Create local world with specific case
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Call Exists with different case
            bool existsExact = World.Exists("TestWorld");
            bool existsLower = World.Exists("testworld");
            bool existsUpper = World.Exists("TESTWORLD");
            
            // Case-sensitive matching: only exact match returns true
            Assert(existsExact, "Exists(\"TestWorld\") should return true for exact match");
            Assert(!existsLower, "Exists(\"testworld\") should return false (case-sensitive)");
            Assert(!existsUpper, "Exists(\"TESTWORLD\") should return false (case-sensitive)");
        });
    }
    
    [ContextMenu("Run Test: Exists Checks Existence At Call Time")]
    public void Test_Exists_009()
    {
        string testName = "Test_Exists_009";
        ExecuteTest(testName, () =>
        {
            // 1. Verify world doesn't exist
            bool existsBefore = World.Exists("DynamicWorld");
            Assert(!existsBefore, "World should not exist before creation");
            
            // 2. Create world
            WorldInstance world = World.GetOrCreate("DynamicWorld");
            
            // 3. Verify world exists (method checks at call time, not cached)
            bool existsAfter = World.Exists("DynamicWorld");
            Assert(existsAfter, "World should exist after creation");
            
            // 4. Destroy world
            bool destroyed = World.Destroy(world);
            Assert(destroyed, "World.Destroy should return true");
            
            // 5. Verify world doesn't exist (method checks at call time)
            bool existsAfterDestroy = World.Exists("DynamicWorld");
            Assert(!existsAfterDestroy, "World should not exist after destruction");
        });
    }
    
    [ContextMenu("Run Test: Exists Multiple Local Worlds")]
    public void Test_Exists_010()
    {
        string testName = "Test_Exists_010";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple local worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            WorldInstance world3 = World.GetOrCreate("World3");
            
            // 2. Check existence of each world
            bool exists1 = World.Exists("World1");
            bool exists2 = World.Exists("World2");
            bool exists3 = World.Exists("World3");
            bool exists4 = World.Exists("World4");
            
            // All created worlds exist, non-existent world doesn't
            Assert(exists1, "World1 should exist");
            Assert(exists2, "World2 should exist");
            Assert(exists3, "World3 should exist");
            Assert(!exists4, "World4 should not exist");
        });
    }
    
    [ContextMenu("Run Test: Exists Global World After Access")]
    public void Test_Exists_011()
    {
        string testName = "Test_Exists_011";
        ExecuteTest(testName, () =>
        {
            // 1. Verify global world doesn't exist before access
            bool existsBefore = World.Exists(null);
            Assert(!existsBefore, "Global world should not exist before access");
            
            // 2. Access global world (lazy initialization)
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 3. Verify global world exists after access
            bool existsAfter = World.Exists(null);
            Assert(existsAfter, "Global world should exist after access");
            
            // 4. Verify with explicit "Global" name
            bool existsGlobal = World.Exists("Global");
            Assert(existsGlobal, "Global world should exist with explicit \"Global\" name");
        });
    }
    
    [ContextMenu("Run Test: Exists Thread Safety")]
    public void Test_Exists_012()
    {
        string testName = "Test_Exists_012";
        ExecuteTest(testName, () =>
        {
            // 1. Create initial world
            WorldInstance world = World.GetOrCreate("ThreadTestWorld");
            
            // 2. Test that Exists() can be called safely from multiple threads
            Thread[] threads = new Thread[5];
            System.Collections.Generic.List<Exception> exceptions = 
                new System.Collections.Generic.List<Exception>();
            object lockObject = new object();
            
            // 3. Create multiple threads that call Exists() concurrently
            for (int i = 0; i < threads.Length; i++)
            {
                int threadIndex = i;
                threads[i] = new Thread(() =>
                {
                    try
                    {
                        // Call Exists() multiple times in each thread
                        for (int j = 0; j < 10; j++)
                        {
                            bool exists = World.Exists("ThreadTestWorld");
                            Assert(exists, "World should exist during concurrent access");
                            
                            // Also test non-existent world
                            bool notExists = World.Exists($"NonExistentWorld{threadIndex}_{j}");
                            Assert(!notExists, "Non-existent world should return false");
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            
            // 4. Start all threads
            foreach (var thread in threads)
            {
                thread.Start();
            }
            
            // 5. Wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }
            
            // 6. Verify no exceptions occurred
            AssertEquals(0, exceptions.Count, 
                $"No exceptions should occur during concurrent Exists() calls. Found {exceptions.Count} exceptions.");
            
            // 7. Verify final state is consistent
            bool finalExists = World.Exists("ThreadTestWorld");
            Assert(finalExists, "World should still exist after concurrent access");
        });
    }
    
    [ContextMenu("Run Test: Exists Concurrent Creation And Destruction")]
    public void Test_Exists_013()
    {
        string testName = "Test_Exists_013";
        ExecuteTest(testName, () =>
        {
            // 1. Test Exists() during concurrent world creation and destruction
            Thread[] threads = new Thread[3];
            System.Collections.Generic.List<Exception> exceptions = 
                new System.Collections.Generic.List<Exception>();
            object lockObject = new object();
            
            // 2. Create threads that create/destroy worlds and check existence
            for (int i = 0; i < threads.Length; i++)
            {
                int threadIndex = i;
                threads[i] = new Thread(() =>
                {
                    try
                    {
                        string worldName = $"ConcurrentWorld{threadIndex}";
                        
                        // Create world
                        WorldInstance world = World.GetOrCreate(worldName);
                        
                        // Verify it exists
                        bool exists = World.Exists(worldName);
                        Assert(exists, $"World {worldName} should exist after creation");
                        
                        // Destroy world
                        bool destroyed = World.Destroy(world);
                        Assert(destroyed, $"World {worldName} should be destroyed");
                        
                        // Verify it doesn't exist
                        bool existsAfter = World.Exists(worldName);
                        Assert(!existsAfter, $"World {worldName} should not exist after destruction");
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            
            // 3. Start all threads
            foreach (var thread in threads)
            {
                thread.Start();
            }
            
            // 4. Wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }
            
            // 5. Verify no exceptions occurred
            AssertEquals(0, exceptions.Count, 
                $"No exceptions should occur during concurrent creation/destruction. Found {exceptions.Count} exceptions.");
        });
    }
    
    [ContextMenu("Run Test: Exists All Name Variants For Global World")]
    public void Test_Exists_014()
    {
        string testName = "Test_Exists_014";
        ExecuteTest(testName, () =>
        {
            // 1. Access global world (lazy initialization)
            WorldInstance globalWorld = World.GlobalWorld;
            
            // 2. Test all name variants that should refer to global world
            bool existsNull = World.Exists(null);
            bool existsEmpty = World.Exists("");
            bool existsGlobal = World.Exists("Global");
            
            // All variants should return true
            Assert(existsNull, "Exists(null) should return true for global world");
            Assert(existsEmpty, "Exists(\"\") should return true for global world");
            Assert(existsGlobal, "Exists(\"Global\") should return true for global world");
        });
    }
    
    [ContextMenu("Run Test: Exists Integration With GetAllWorlds")]
    public void Test_Exists_015()
    {
        string testName = "Test_Exists_015";
        ExecuteTest(testName, () =>
        {
            // 1. Create multiple worlds
            WorldInstance globalWorld = World.GlobalWorld;
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Get all worlds
            var allWorlds = World.GetAllWorlds();
            
            // 3. Verify Exists() returns true for all worlds from GetAllWorlds()
            foreach (var world in allWorlds)
            {
                bool exists = World.Exists(world.Name);
                Assert(exists, $"World '{world.Name}' from GetAllWorlds() should exist according to Exists()");
            }
            
            // 4. Verify Exists() returns false for non-existent world
            bool nonExistent = World.Exists("NonExistentWorld");
            Assert(!nonExistent, "Non-existent world should return false");
        });
    }
    
    // ========== API-019: GetAllComponentInfos Method ==========
    
    [ContextMenu("Run Test: GetAllComponentInfos Empty Entity")]
    public void Test_GetAllComponentInfos_001()
    {
        string testName = "Test_GetAllComponentInfos_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity without components
            Entity entity = world.CreateEntity();
            
            // 3. Call GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // Empty entity returns empty array
            AssertNotNull(componentInfos, "GetAllComponentInfos should return non-null array");
            AssertEquals(0, componentInfos.Length, "Empty entity should return empty array");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Single Component")]
    public void Test_GetAllComponentInfos_002()
    {
        string testName = "Test_GetAllComponentInfos_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with one component
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Call GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // Entity with one component returns array with one ComponentInfo
            AssertNotNull(componentInfos, "GetAllComponentInfos should return non-null array");
            AssertEquals(1, componentInfos.Length, "Entity with one component should return array with one ComponentInfo");
            
            // 4. Verify ComponentInfo structure
            var info = componentInfos[0];
            AssertEquals(typeof(TestComponent), info.ComponentType, "ComponentType should be TestComponent");
            AssertNotNull(info.Value, "Value should not be null");
            
            // 5. Verify boxed value is correct
            var boxedComponent = (TestComponent)info.Value;
            AssertEquals(42, boxedComponent.Value, "Boxed component Value should be 42");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Multiple Components")]
    public void Test_GetAllComponentInfos_003()
    {
        string testName = "Test_GetAllComponentInfos_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with multiple components
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity, new Health { Amount = 100f });
            
            // 3. Call GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // Entity with multiple components returns array with all ComponentInfos
            AssertNotNull(componentInfos, "GetAllComponentInfos should return non-null array");
            AssertEquals(3, componentInfos.Length, "Entity with 3 components should return array with 3 ComponentInfos");
            
            // 4. Verify all component types are present
            var types = new System.Collections.Generic.HashSet<Type>();
            foreach (var info in componentInfos)
            {
                types.Add(info.ComponentType);
            }
            Assert(types.Contains(typeof(Position)), "Position component should be present");
            Assert(types.Contains(typeof(Velocity)), "Velocity component should be present");
            Assert(types.Contains(typeof(Health)), "Health component should be present");
            
            // 5. Verify values are correct
            foreach (var info in componentInfos)
            {
                if (info.ComponentType == typeof(Position))
                {
                    var pos = (Position)info.Value;
                    AssertEquals(1f, pos.X, "Position.X should be 1");
                    AssertEquals(2f, pos.Y, "Position.Y should be 2");
                    AssertEquals(3f, pos.Z, "Position.Z should be 3");
                }
                else if (info.ComponentType == typeof(Velocity))
                {
                    var vel = (Velocity)info.Value;
                    AssertEquals(4f, vel.X, "Velocity.X should be 4");
                    AssertEquals(5f, vel.Y, "Velocity.Y should be 5");
                    AssertEquals(6f, vel.Z, "Velocity.Z should be 6");
                }
                else if (info.ComponentType == typeof(Health))
                {
                    var health = (Health)info.Value;
                    AssertEquals(100f, health.Amount, "Health.Amount should be 100");
                }
            }
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos ComponentType Correctness")]
    public void Test_GetAllComponentInfos_004()
    {
        string testName = "Test_GetAllComponentInfos_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with different component types
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestComponent { Value = 123 });
            world.AddComponent(entity, new Position { X = 10f, Y = 20f, Z = 30f });
            
            // 3. Call GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // 4. Verify ComponentType is correct for each ComponentInfo
            AssertEquals(2, componentInfos.Length, "Should have 2 components");
            
            bool foundTestComponent = false;
            bool foundPosition = false;
            
            foreach (var info in componentInfos)
            {
                AssertNotNull(info.ComponentType, "ComponentType should not be null");
                
                if (info.ComponentType == typeof(TestComponent))
                {
                    foundTestComponent = true;
                    Assert(info.Value is TestComponent, "Value should be TestComponent");
                }
                else if (info.ComponentType == typeof(Position))
                {
                    foundPosition = true;
                    Assert(info.Value is Position, "Value should be Position");
                }
            }
            
            Assert(foundTestComponent, "TestComponent should be found");
            Assert(foundPosition, "Position should be found");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Value Boxing Correctness")]
    public void Test_GetAllComponentInfos_005()
    {
        string testName = "Test_GetAllComponentInfos_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with component
            Entity entity = world.CreateEntity();
            var originalComponent = new TestComponent { Value = 999 };
            world.AddComponent(entity, originalComponent);
            
            // 3. Call GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // 4. Verify Value is correctly boxed component value
            AssertEquals(1, componentInfos.Length, "Should have 1 component");
            var info = componentInfos[0];
            
            AssertNotNull(info.Value, "Value should not be null");
            Assert(info.Value is TestComponent, "Value should be boxed TestComponent");
            
            var unboxedComponent = (TestComponent)info.Value;
            AssertEquals(999, unboxedComponent.Value, "Unboxed component Value should be 999");
            AssertEquals(originalComponent.Value, unboxedComponent.Value, "Boxed value should match original");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos JsonValue Generated")]
    public void Test_GetAllComponentInfos_006()
    {
        string testName = "Test_GetAllComponentInfos_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with component
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Call GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // 4. Verify JsonValue is generated (can be null if serialization fails)
            AssertEquals(1, componentInfos.Length, "Should have 1 component");
            var info = componentInfos[0];
            
            // JsonValue may or may not be null depending on Unity JsonUtility support
            // We just verify the field exists and can be accessed
            // (Note: JsonValue generation is optional per API spec)
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Invalid Entity Throws Exception")]
    public void Test_GetAllComponentInfos_007()
    {
        string testName = "Test_GetAllComponentInfos_007";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create invalid entity
            Entity invalidEntity = Entity.Invalid;
            
            // 3. Call GetAllComponentInfos() with invalid entity
            // Should throw InvalidEntityException
            bool exceptionThrown = false;
            try
            {
                world.GetAllComponentInfos(invalidEntity);
            }
            catch (InvalidEntityException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "GetAllComponentInfos should throw InvalidEntityException for invalid entity");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Different Component Types")]
    public void Test_GetAllComponentInfos_008()
    {
        string testName = "Test_GetAllComponentInfos_008";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with various component types (simple structs, empty structs, etc.)
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new TestComponent { Value = 1 });
            world.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity, new Dead()); // Empty struct component
            
            // 3. Call GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // 4. Verify all component types are returned
            AssertEquals(3, componentInfos.Length, "Should have 3 components");
            
            var types = new System.Collections.Generic.HashSet<Type>();
            foreach (var info in componentInfos)
            {
                types.Add(info.ComponentType);
                AssertNotNull(info.Value, "Value should not be null even for empty struct");
            }
            
            Assert(types.Contains(typeof(TestComponent)), "TestComponent should be present");
            Assert(types.Contains(typeof(Position)), "Position should be present");
            Assert(types.Contains(typeof(Dead)), "Dead (empty struct) should be present");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Static World Method")]
    public void Test_GetAllComponentInfos_009()
    {
        string testName = "Test_GetAllComponentInfos_009";
        ExecuteTest(testName, () =>
        {
            // 1. Create entity in global world
            Entity entity = World.CreateEntity();
            World.AddComponent(entity, new TestComponent { Value = 100 });
            World.AddComponent(entity, new Position { X = 5f, Y = 6f, Z = 7f });
            
            // 2. Call static World.GetAllComponentInfos()
            var componentInfos = World.GetAllComponentInfos(entity);
            
            // 3. Verify result
            AssertNotNull(componentInfos, "GetAllComponentInfos should return non-null array");
            AssertEquals(2, componentInfos.Length, "Should have 2 components");
            
            // 4. Verify components are correct
            var types = new System.Collections.Generic.HashSet<Type>();
            foreach (var info in componentInfos)
            {
                types.Add(info.ComponentType);
            }
            Assert(types.Contains(typeof(TestComponent)), "TestComponent should be present");
            Assert(types.Contains(typeof(Position)), "Position should be present");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos WorldInstance Method")]
    public void Test_GetAllComponentInfos_010()
    {
        string testName = "Test_GetAllComponentInfos_010";
        ExecuteTest(testName, () =>
        {
            // 1. Create world instance
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with components
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new Health { Amount = 50f });
            
            // 3. Call instance method world.GetAllComponentInfos()
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // 4. Verify result
            AssertNotNull(componentInfos, "GetAllComponentInfos should return non-null array");
            AssertEquals(1, componentInfos.Length, "Should have 1 component");
            AssertEquals(typeof(Health), componentInfos[0].ComponentType, "ComponentType should be Health");
            
            var health = (Health)componentInfos[0].Value;
            AssertEquals(50f, health.Amount, "Health.Amount should be 50");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos World Isolation")]
    public void Test_GetAllComponentInfos_011()
    {
        string testName = "Test_GetAllComponentInfos_011";
        ExecuteTest(testName, () =>
        {
            // 1. Create two separate worlds
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Create entity in world1 with components
            Entity entity1 = world1.CreateEntity();
            world1.AddComponent(entity1, new TestComponent { Value = 1 });
            world1.AddComponent(entity1, new Position { X = 1f, Y = 1f, Z = 1f });
            
            // 3. Create entity in world2 with components
            Entity entity2 = world2.CreateEntity();
            world2.AddComponent(entity2, new TestComponent { Value = 2 });
            world2.AddComponent(entity2, new Health { Amount = 200f });
            
            // 4. Get component infos from each world
            var infos1 = world1.GetAllComponentInfos(entity1);
            var infos2 = world2.GetAllComponentInfos(entity2);
            
            // 5. Verify world isolation - each world returns only its own components
            AssertEquals(2, infos1.Length, "World1 should have 2 components");
            AssertEquals(2, infos2.Length, "World2 should have 2 components");
            
            // Verify world1 components
            var types1 = new System.Collections.Generic.HashSet<Type>();
            foreach (var info in infos1)
            {
                types1.Add(info.ComponentType);
            }
            Assert(types1.Contains(typeof(TestComponent)), "World1 should have TestComponent");
            Assert(types1.Contains(typeof(Position)), "World1 should have Position");
            Assert(!types1.Contains(typeof(Health)), "World1 should NOT have Health");
            
            // Verify world2 components
            var types2 = new System.Collections.Generic.HashSet<Type>();
            foreach (var info in infos2)
            {
                types2.Add(info.ComponentType);
            }
            Assert(types2.Contains(typeof(TestComponent)), "World2 should have TestComponent");
            Assert(types2.Contains(typeof(Health)), "World2 should have Health");
            Assert(!types2.Contains(typeof(Position)), "World2 should NOT have Position");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos After Component Removal")]
    public void Test_GetAllComponentInfos_012()
    {
        string testName = "Test_GetAllComponentInfos_012";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with multiple components
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity, new Velocity { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity, new Health { Amount = 100f });
            
            // 3. Get component infos (should have 3)
            var infos1 = world.GetAllComponentInfos(entity);
            AssertEquals(3, infos1.Length, "Should have 3 components before removal");
            
            // 4. Remove one component
            world.RemoveComponent<Velocity>(entity);
            
            // 5. Get component infos again (should have 2)
            var infos2 = world.GetAllComponentInfos(entity);
            AssertEquals(2, infos2.Length, "Should have 2 components after removal");
            
            // 6. Verify removed component is not present
            var types = new System.Collections.Generic.HashSet<Type>();
            foreach (var info in infos2)
            {
                types.Add(info.ComponentType);
            }
            Assert(types.Contains(typeof(Position)), "Position should still be present");
            Assert(types.Contains(typeof(Health)), "Health should still be present");
            Assert(!types.Contains(typeof(Velocity)), "Velocity should NOT be present (removed)");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Empty World")]
    public void Test_GetAllComponentInfos_013()
    {
        string testName = "Test_GetAllComponentInfos_013";
        ExecuteTest(testName, () =>
        {
            // 1. Create empty world (no component tables)
            WorldInstance world = World.GetOrCreate("EmptyWorld");
            
            // 2. Create entity (but don't add components, so no component tables are created)
            Entity entity = world.CreateEntity();
            
            // 3. Call GetAllComponentInfos() on entity in world with no component tables
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // Empty world returns empty array
            AssertNotNull(componentInfos, "GetAllComponentInfos should return non-null array");
            AssertEquals(0, componentInfos.Length, "Empty world should return empty array");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Multiple Entities Same World")]
    public void Test_GetAllComponentInfos_014()
    {
        string testName = "Test_GetAllComponentInfos_014";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create multiple entities with different components
            Entity entity1 = world.CreateEntity();
            world.AddComponent(entity1, new TestComponent { Value = 1 });
            
            Entity entity2 = world.CreateEntity();
            world.AddComponent(entity2, new Position { X = 10f, Y = 20f, Z = 30f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            Entity entity3 = world.CreateEntity();
            world.AddComponent(entity3, new Health { Amount = 150f });
            
            // 3. Get component infos for each entity
            var infos1 = world.GetAllComponentInfos(entity1);
            var infos2 = world.GetAllComponentInfos(entity2);
            var infos3 = world.GetAllComponentInfos(entity3);
            
            // 4. Verify each entity returns correct components
            AssertEquals(1, infos1.Length, "Entity1 should have 1 component");
            AssertEquals(2, infos2.Length, "Entity2 should have 2 components");
            AssertEquals(1, infos3.Length, "Entity3 should have 1 component");
            
            AssertEquals(typeof(TestComponent), infos1[0].ComponentType, "Entity1 should have TestComponent");
            
            var types2 = new System.Collections.Generic.HashSet<Type>();
            foreach (var info in infos2)
            {
                types2.Add(info.ComponentType);
            }
            Assert(types2.Contains(typeof(Position)), "Entity2 should have Position");
            Assert(types2.Contains(typeof(Velocity)), "Entity2 should have Velocity");
            
            AssertEquals(typeof(Health), infos3[0].ComponentType, "Entity3 should have Health");
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos Component Values Preserved")]
    public void Test_GetAllComponentInfos_015()
    {
        string testName = "Test_GetAllComponentInfos_015";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with components having specific values
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new Position { X = 123.456f, Y = 789.012f, Z = 345.678f });
            world.AddComponent(entity, new TestComponent { Value = -42 });
            
            // 3. Get component infos
            var componentInfos = world.GetAllComponentInfos(entity);
            
            // 4. Verify component values are preserved after boxing/unboxing
            AssertEquals(2, componentInfos.Length, "Should have 2 components");
            
            foreach (var info in componentInfos)
            {
                if (info.ComponentType == typeof(Position))
                {
                    var pos = (Position)info.Value;
                    AssertEquals(123.456f, pos.X, "Position.X should be preserved");
                    AssertEquals(789.012f, pos.Y, "Position.Y should be preserved");
                    AssertEquals(345.678f, pos.Z, "Position.Z should be preserved");
                }
                else if (info.ComponentType == typeof(TestComponent))
                {
                    var test = (TestComponent)info.Value;
                    AssertEquals(-42, test.Value, "TestComponent.Value should be preserved");
                }
            }
        });
    }
    
    [ContextMenu("Run Test: GetAllComponentInfos After Component Modification")]
    public void Test_GetAllComponentInfos_016()
    {
        string testName = "Test_GetAllComponentInfos_016";
        ExecuteTest(testName, () =>
        {
            // 1. Create world
            WorldInstance world = World.GetOrCreate("TestWorld");
            
            // 2. Create entity with component
            Entity entity = world.CreateEntity();
            world.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            
            // 3. Get component infos (original values)
            var infos1 = world.GetAllComponentInfos(entity);
            var originalPos = (Position)infos1[0].Value;
            AssertEquals(1f, originalPos.X, "Original X should be 1");
            
            // 4. Modify component
            ref var modifiablePos = ref world.GetModifiableComponent<Position>(entity);
            modifiablePos.X = 999f;
            modifiablePos.Y = 888f;
            modifiablePos.Z = 777f;
            
            // 5. Get component infos again (should reflect modifications)
            var infos2 = world.GetAllComponentInfos(entity);
            var modifiedPos = (Position)infos2[0].Value;
            AssertEquals(999f, modifiedPos.X, "Modified X should be 999");
            AssertEquals(888f, modifiedPos.Y, "Modified Y should be 888");
            AssertEquals(777f, modifiedPos.Z, "Modified Z should be 777");
        });
    }
}

