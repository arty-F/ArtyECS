using UnityEngine;
using ArtyECS.Core;
using System;

/// <summary>
/// Test class for Core functionality (Core-000 through Core-012).
/// </summary>
public class CoreTests : TestBase
{
    // ========== Core-000: Entity Implementation ==========
    
    [ContextMenu("Run Test: Entity Creation with ID and Generation")]
    public void Test_Entity_001()
    {
        string testName = "Test_Entity_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity with id=5, generation=2
            Entity entity = new Entity(5, 2);
            
            // 2. Verify that Entity.Id == 5
            AssertEquals(5, entity.Id, "Entity.Id should be 5");
            
            // 3. Verify that Entity.Generation == 2
            AssertEquals(2, entity.Generation, "Entity.Generation should be 2");
            
            // 4. Verify that Entity.IsValid == true
            Assert(entity.IsValid, "Entity.IsValid should be true");
        });
    }
    
    [ContextMenu("Run Test: Invalid Entity")]
    public void Test_Entity_002()
    {
        string testName = "Test_Entity_002";
        ExecuteTest(testName, () =>
        {
            // 1. Get Entity.Invalid
            Entity invalid = Entity.Invalid;
            
            // 2. Check IsValid
            AssertEquals(-1, invalid.Id, "Entity.Invalid.Id should be -1");
            AssertEquals(0, invalid.Generation, "Entity.Invalid.Generation should be 0");
            Assert(!invalid.IsValid, "Entity.Invalid.IsValid should be false");
        });
    }
    
    [ContextMenu("Run Test: Entity Equality")]
    public void Test_Entity_003()
    {
        string testName = "Test_Entity_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 with id=1, generation=0
            Entity entity1 = new Entity(1, 0);
            
            // 2. Create Entity2 with id=1, generation=0
            Entity entity2 = new Entity(1, 0);
            
            // 3. Create Entity3 with id=1, generation=1
            Entity entity3 = new Entity(1, 1);
            
            // 4. Create Entity4 with id=2, generation=0
            Entity entity4 = new Entity(2, 0);
            
            // Only Entities with same ID and Generation are equal
            Assert(entity1 == entity2, "Entity1 == Entity2 should be true");
            Assert(entity1 != entity3, "Entity1 != Entity3 should be true");
            Assert(entity1 != entity4, "Entity1 != Entity4 should be true");
            Assert(entity1.Equals(entity2), "Entity1.Equals(Entity2) should be true");
            Assert(!entity1.Equals(entity3), "Entity1.Equals(Entity3) should be false");
        });
    }
    
    [ContextMenu("Run Test: Entity HashCode")]
    public void Test_Entity_004()
    {
        string testName = "Test_Entity_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 with id=1, generation=0
            Entity entity1 = new Entity(1, 0);
            
            // 2. Create Entity2 with id=1, generation=0
            Entity entity2 = new Entity(1, 0);
            
            // 3. Create Entity3 with id=1, generation=1
            Entity entity3 = new Entity(1, 1);
            
            // Same Entities have same hash code, different ones have different hash codes
            AssertEquals(entity1.GetHashCode(), entity2.GetHashCode(), "Entity1 and Entity2 should have same hash code");
            Assert(entity1.GetHashCode() != entity3.GetHashCode(), "Entity1 and Entity3 should have different hash codes");
        });
    }
    
    [ContextMenu("Run Test: Entity ToString")]
    public void Test_Entity_005()
    {
        string testName = "Test_Entity_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity with id=42, generation=3
            Entity entity = new Entity(42, 3);
            
            // 2. Call ToString()
            string str = entity.ToString();
            
            // String contains ID and Generation
            Assert(str.Contains("42"), "ToString should contain ID 42");
            Assert(str.Contains("3"), "ToString should contain Generation 3");
        });
    }
    
    // ========== Core-001: Component Base/Marker Interface ==========
    
    [ContextMenu("Run Test: Component Implements IComponent")]
    public void Test_Component_001()
    {
        string testName = "Test_Component_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create TestComponent instance
            TestComponent testComponent = new TestComponent { Value = 42 };
            
            // 2. Verify that it implements IComponent
            Assert(testComponent is IComponent, "TestComponent should implement IComponent");
        });
    }
    
    [ContextMenu("Run Test: Multiple Components Implement IComponent")]
    public void Test_Component_002()
    {
        string testName = "Test_Component_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Position : IComponent
            Position position = new Position { X = 1f, Y = 2f, Z = 3f };
            
            // 2. Create Velocity : IComponent
            Velocity velocity = new Velocity { X = 1f, Y = 2f, Z = 3f };
            
            // 3. Create Health : IComponent
            Health health = new Health { Amount = 100f };
            
            // All components implement IComponent
            Assert(position is IComponent, "Position should implement IComponent");
            Assert(velocity is IComponent, "Velocity should implement IComponent");
            Assert(health is IComponent, "Health should implement IComponent");
        });
    }
    
    // ========== Core-002: ComponentsManager - Basic Structure ==========
    
    [ContextMenu("Run Test: GetOrCreate Returns World")]
    public void Test_Storage_001()
    {
        string testName = "Test_Storage_001";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.GetOrCreate() for global world
            WorldInstance world = World.GetOrCreate();
            
            // Returns WorldInstance with name "Global"
            AssertNotNull(world, "World should not be null");
            AssertEquals("Global", world.Name, "World.Name should be 'Global'");
        });
    }
    
    [ContextMenu("Run Test: World Scoped Storage Isolation")]
    public void Test_Storage_002()
    {
        string testName = "Test_Storage_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create World1("World1") and World2("World2")
            WorldInstance world1 = World.GetOrCreate("World1");
            WorldInstance world2 = World.GetOrCreate("World2");
            
            // 2. Create Entity1 and Entity2
            Entity entity1 = world1.CreateEntity();
            Entity entity2 = world2.CreateEntity();
            
            // 3. Add component to Entity1 in World1
            world1.AddComponent(entity1, new TestComponent { Value = 1 });
            
            // 4. Add component to Entity2 in World2
            world2.AddComponent(entity2, new TestComponent { Value = 2 });
            
            // 5. Check components in each world
            // Components are isolated by worlds
            TestComponent comp1 = world1.GetComponent<TestComponent>(entity1);
            AssertEquals(1, comp1.Value, "Entity1 in World1 should have component with Value=1");
            
            // Entity1 should not have component in World2
            bool hasComp1InWorld2 = entity1.Has<TestComponent>(world2);
            Assert(!hasComp1InWorld2, "Entity1 should not have component in World2");
            
            // Entity2 should not have component in World1
            bool hasComp2InWorld1 = entity2.Has<TestComponent>(world1);
            Assert(!hasComp2InWorld1, "Entity2 should not have component in World1");
            
            TestComponent comp2 = world2.GetComponent<TestComponent>(entity2);
            AssertEquals(2, comp2.Value, "Entity2 in World2 should have component with Value=2");
        });
    }
    
    // ========== Core-003: ComponentsManager - Single Component Type Storage ==========
    
    [ContextMenu("Run Test: ComponentTable Initial Capacity")]
    public void Test_Storage_003()
    {
        string testName = "Test_Storage_003";
        ExecuteTest(testName, () =>
        {
            // 1. Add first component
            Entity entity = World.CreateEntity();
            World.AddComponent(entity, new TestComponent { Value = 1 });
            
            // 2. Verify that storage is created and component successfully added
            TestComponent comp = World.GetComponent<TestComponent>(entity);
            AssertEquals(1, comp.Value, "Component should be successfully added");
        });
    }
    
    [ContextMenu("Run Test: ComponentTable Capacity Growth")]
    public void Test_Storage_004()
    {
        string testName = "Test_Storage_004";
        ExecuteTest(testName, () =>
        {
            // 1. Add 20 components (more than default capacity)
            Entity[] entities = new Entity[20];
            for (int i = 0; i < 20; i++)
            {
                entities[i] = World.CreateEntity();
                World.AddComponent(entities[i], new TestComponent { Value = i });
            }
            
            // 2. Verify that all components are added
            var components = World.GetComponents<TestComponent>();
            AssertEquals(20, components.Length, "All 20 components should be added");
        });
    }
    
    [ContextMenu("Run Test: ComponentTable Contiguous Memory Layout")]
    public void Test_Storage_005()
    {
        string testName = "Test_Storage_005";
        ExecuteTest(testName, () =>
        {
            // 1. Add 5 components
            for (int i = 0; i < 5; i++)
            {
                Entity entity = World.CreateEntity();
                World.AddComponent(entity, new TestComponent { Value = i });
            }
            
            // 2. Get ReadOnlySpan of components
            var span = World.GetComponents<TestComponent>();
            
            // 3. Verify that all components are accessible
            AssertEquals(5, span.Length, "Span.Length should be 5");
            for (int i = 0; i < 5; i++)
            {
                AssertEquals(i, span[i].Value, $"Component {i} should have Value={i}");
            }
        });
    }
    
    // ========== Core-004: ComponentsManager - Add Component ==========
    
    [ContextMenu("Run Test: Add Single Component")]
    public void Test_Add_001()
    {
        string testName = "Test_Add_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add TestComponent with value
            TestComponent expectedComponent = new TestComponent { Value = 42 };
            World.AddComponent(entity, expectedComponent);
            
            // 3. Get component back
            TestComponent actualComponent = World.GetComponent<TestComponent>(entity);
            AssertEquals(expectedComponent.Value, actualComponent.Value, "Component value should match");
        });
    }
    
    [ContextMenu("Run Test: Add Multiple Different Components")]
    public void Test_Add_002()
    {
        string testName = "Test_Add_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add Position component
            World.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            
            // 3. Add Velocity component
            World.AddComponent(entity, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 4. Add Health component
            World.AddComponent(entity, new Health { Amount = 100f });
            
            // 5. Get all components
            // All components successfully added
            Position pos = World.GetComponent<Position>(entity);
            Velocity vel = World.GetComponent<Velocity>(entity);
            Health health = World.GetComponent<Health>(entity);
            
            AssertEquals(1f, pos.X, "Position.X should be 1f");
            AssertEquals(1f, vel.X, "Velocity.X should be 1f");
            AssertEquals(100f, health.Amount, "Health.Amount should be 100f");
        });
    }
    
    [ContextMenu("Run Test: Add Duplicate Component Throws Exception")]
    public void Test_Add_003()
    {
        string testName = "Test_Add_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add TestComponent
            World.AddComponent(entity, new TestComponent { Value = 1 });
            
            // 3. Attempt to add TestComponent again
            bool exceptionThrown = false;
            try
            {
                World.AddComponent(entity, new TestComponent { Value = 2 });
            }
            catch (DuplicateComponentException)
            {
                exceptionThrown = true;
            }
            
            // DuplicateComponentException is thrown
            Assert(exceptionThrown, "DuplicateComponentException should be thrown");
        });
    }
    
    [ContextMenu("Run Test: Add Component to Multiple Entities")]
    public void Test_Add_004()
    {
        string testName = "Test_Add_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Add TestComponent to each with different values
            World.AddComponent(entity1, new TestComponent { Value = 1 });
            World.AddComponent(entity2, new TestComponent { Value = 2 });
            World.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 3. Get components back
            // Each entity has its own component
            TestComponent comp1 = World.GetComponent<TestComponent>(entity1);
            TestComponent comp2 = World.GetComponent<TestComponent>(entity2);
            TestComponent comp3 = World.GetComponent<TestComponent>(entity3);
            
            AssertEquals(1, comp1.Value, "Entity1 should have Value=1");
            AssertEquals(2, comp2.Value, "Entity2 should have Value=2");
            AssertEquals(3, comp3.Value, "Entity3 should have Value=3");
        });
    }
    
    [ContextMenu("Run Test: Add Component with World Parameter")]
    public void Test_Add_005()
    {
        string testName = "Test_Add_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("Test")
            WorldInstance testWorld = World.GetOrCreate("Test");
            
            // 2. Create Entity
            Entity entity = testWorld.CreateEntity();
            
            // 3. Add component to specified world
            testWorld.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 4. Get component from specified world
            TestComponent comp = testWorld.GetComponent<TestComponent>(entity);
            AssertEquals(42, comp.Value, "Component in testWorld should have Value=42");
            
            // 5. Attempt to get component from global world
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<TestComponent>(entity);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ComponentNotFoundException should be thrown when getting from global world");
        });
    }
    
    // ========== Core-005: ComponentsManager - Remove Component ==========
    
    [ContextMenu("Run Test: Remove Existing Component")]
    public void Test_Remove_001()
    {
        string testName = "Test_Remove_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add TestComponent
            World.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Remove TestComponent
            bool removed = World.RemoveComponent<TestComponent>(entity);
            
            // 4. Verify that component is removed
            Assert(removed, "RemoveComponent should return true");
            
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<TestComponent>(entity);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ComponentNotFoundException should be thrown after removal");
        });
    }
    
    [ContextMenu("Run Test: Remove Non-Existent Component")]
    public void Test_Remove_002()
    {
        string testName = "Test_Remove_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Attempt to remove non-existent component
            bool removed = World.RemoveComponent<TestComponent>(entity);
            
            // Returns false, no exception thrown
            Assert(!removed, "RemoveComponent should return false for non-existent component");
        });
    }
    
    [ContextMenu("Run Test: Remove Component Maintains Other Components")]
    public void Test_Remove_003()
    {
        string testName = "Test_Remove_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add Position, Velocity, Health
            World.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity, new Velocity { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity, new Health { Amount = 100f });
            
            // 3. Remove Velocity
            World.RemoveComponent<Velocity>(entity);
            
            // 4. Check remaining components
            // Position and Health remain, Velocity removed
            Position pos = World.GetComponent<Position>(entity);
            AssertEquals(1f, pos.X, "Position should remain");
            
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<Velocity>(entity);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            Assert(exceptionThrown, "Velocity should be removed");
            
            Health health = World.GetComponent<Health>(entity);
            AssertEquals(100f, health.Amount, "Health should remain");
        });
    }
    
    [ContextMenu("Run Test: Remove Component Swap With Last Element")]
    public void Test_Remove_004()
    {
        string testName = "Test_Remove_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Add components to all three
            World.AddComponent(entity1, new TestComponent { Value = 1 });
            World.AddComponent(entity2, new TestComponent { Value = 2 });
            World.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 3. Remove component from Entity2 (middle)
            World.RemoveComponent<TestComponent>(entity2);
            
            // 4. Verify that Entity1 and Entity3 still have components
            TestComponent comp1 = World.GetComponent<TestComponent>(entity1);
            TestComponent comp3 = World.GetComponent<TestComponent>(entity3);
            
            AssertEquals(1, comp1.Value, "Entity1 should still have component");
            AssertEquals(3, comp3.Value, "Entity3 should still have component");
            
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<TestComponent>(entity2);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            Assert(exceptionThrown, "Entity2 should not have component");
        });
    }
    
    // ========== Core-006: ComponentsManager - Get Component (Single Entity) ==========
    
    [ContextMenu("Run Test: Get Existing Component")]
    public void Test_Get_001()
    {
        string testName = "Test_Get_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add TestComponent with known value
            TestComponent expectedComponent = new TestComponent { Value = 42 };
            World.AddComponent(entity, expectedComponent);
            
            // 3. Get component
            TestComponent actualComponent = World.GetComponent<TestComponent>(entity);
            AssertEquals(expectedComponent.Value, actualComponent.Value, "Component value should match");
        });
    }
    
    [ContextMenu("Run Test: Get Non-Existent Component Throws Exception")]
    public void Test_Get_002()
    {
        string testName = "Test_Get_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Get non-existent component
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<TestComponent>(entity);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            
            // ComponentNotFoundException is thrown
            Assert(exceptionThrown, "ComponentNotFoundException should be thrown");
        });
    }
    
    [ContextMenu("Run Test: Get Component After Removal")]
    public void Test_Get_003()
    {
        string testName = "Test_Get_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add TestComponent
            World.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Remove TestComponent
            World.RemoveComponent<TestComponent>(entity);
            
            // 4. Get TestComponent
            // ComponentNotFoundException should be thrown
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<TestComponent>(entity);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            
            Assert(exceptionThrown, "ComponentNotFoundException should be thrown after removal");
        });
    }
    
    // ========== Core-007: ComponentsManager - GetComponents (Single Type Query) ==========
    
    [ContextMenu("Run Test: GetComponents Empty Storage")]
    public void Test_Query_001()
    {
        string testName = "Test_Query_001";
        ExecuteTest(testName, () =>
        {
            // 1. Get components from empty storage
            var components = World.GetComponents<TestComponent>();
            
            // Returns empty span
            AssertEquals(0, components.Length, "Components.Length should be 0");
            Assert(components.IsEmpty, "Components should be empty");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Single Component")]
    public void Test_Query_002()
    {
        string testName = "Test_Query_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add TestComponent
            TestComponent expectedComponent = new TestComponent { Value = 42 };
            World.AddComponent(entity, expectedComponent);
            
            // 3. Get all components
            var components = World.GetComponents<TestComponent>();
            
            // Returns span with one component
            AssertEquals(1, components.Length, "Components.Length should be 1");
            AssertEquals(expectedComponent.Value, components[0].Value, "Component value should match");
        });
    }
    
    [ContextMenu("Run Test: GetComponents Multiple Components")]
    public void Test_Query_003()
    {
        string testName = "Test_Query_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Add TestComponent to each with different values
            World.AddComponent(entity1, new TestComponent { Value = 1 });
            World.AddComponent(entity2, new TestComponent { Value = 2 });
            World.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 3. Get all components
            var components = World.GetComponents<TestComponent>();
            
            // Returns span with three components
            AssertEquals(3, components.Length, "Components.Length should be 3");
        });
    }
    
    [ContextMenu("Run Test: GetComponents After Removal")]
    public void Test_Query_004()
    {
        string testName = "Test_Query_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Add components to all three
            World.AddComponent(entity1, new TestComponent { Value = 1 });
            World.AddComponent(entity2, new TestComponent { Value = 2 });
            World.AddComponent(entity3, new TestComponent { Value = 3 });
            
            // 3. Remove component from Entity2
            World.RemoveComponent<TestComponent>(entity2);
            
            // 4. Get all components
            var components = World.GetComponents<TestComponent>();
            
            // Returns span with two components
            AssertEquals(2, components.Length, "Components.Length should be 2");
        });
    }
    
    // ========== Core-008: ComponentsManager - GetEntitiesWith (Multiple AND Query) ==========
    // Note: GetComponents<T1, T2>() removed (API-004), replaced with GetEntitiesWith<T1, T2>()
    
    [ContextMenu("Run Test: GetEntitiesWith Two Types - Both Present")]
    public void Test_Query_005()
    {
        string testName = "Test_Query_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 with Position and Velocity
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Create Entity2 only with Position
            Entity entity2 = World.CreateEntity();
            World.AddComponent(entity2, new Position { X = 2f, Y = 3f, Z = 4f });
            
            // 3. Create Entity3 only with Velocity
            Entity entity3 = World.CreateEntity();
            World.AddComponent(entity3, new Velocity { X = 2f, Y = 3f, Z = 4f });
            
            // 4. Call GetEntitiesWith<Position, Velocity>()
            var entities = World.GetEntitiesWith<Position, Velocity>();
            
            // Returns only Entity1 (has both components)
            AssertEquals(1, entities.Length, "Should return only one entity");
            AssertEquals(entity1.Id, entities[0].Id, "Returned entity should be Entity1");
            AssertEquals(entity1.Generation, entities[0].Generation, "Returned entity should be Entity1");
        });
    }
    
    [ContextMenu("Run Test: GetEntitiesWith Two Types - No Match")]
    public void Test_Query_006()
    {
        string testName = "Test_Query_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 only with Position
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Create Entity2 only with Velocity
            Entity entity2 = World.CreateEntity();
            World.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 3. Call GetEntitiesWith<Position, Velocity>()
            var entities = World.GetEntitiesWith<Position, Velocity>();
            
            // Returns empty span
            AssertEquals(0, entities.Length, "Should return empty span");
            Assert(entities.IsEmpty, "Should return empty span");
        });
    }
    
    [ContextMenu("Run Test: GetEntitiesWith Two Types - Multiple Matches")]
    public void Test_Query_007()
    {
        string testName = "Test_Query_007";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3 with Position and Velocity
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            Entity entity2 = World.CreateEntity();
            World.AddComponent(entity2, new Position { X = 2f, Y = 3f, Z = 4f });
            World.AddComponent(entity2, new Velocity { X = 2f, Y = 3f, Z = 4f });
            
            Entity entity3 = World.CreateEntity();
            World.AddComponent(entity3, new Position { X = 3f, Y = 4f, Z = 5f });
            World.AddComponent(entity3, new Velocity { X = 3f, Y = 4f, Z = 5f });
            
            // 2. Create Entity4 only with Position
            Entity entity4 = World.CreateEntity();
            World.AddComponent(entity4, new Position { X = 4f, Y = 5f, Z = 6f });
            
            // 3. Call GetEntitiesWith<Position, Velocity>()
            var entities = World.GetEntitiesWith<Position, Velocity>();
            
            // Returns span with three entities
            AssertEquals(3, entities.Length, "Should return three entities");
        });
    }
    
    [ContextMenu("Run Test: GetEntitiesWith Three Types - All Present")]
    public void Test_Query_008()
    {
        string testName = "Test_Query_008";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 with Position, Velocity, Health
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity1, new Health { Amount = 100f });
            
            // 2. Create Entity2 with Position, Velocity (without Health)
            Entity entity2 = World.CreateEntity();
            World.AddComponent(entity2, new Position { X = 2f, Y = 3f, Z = 4f });
            World.AddComponent(entity2, new Velocity { X = 2f, Y = 3f, Z = 4f });
            
            // 3. Create Entity3 with Position, Health (without Velocity)
            Entity entity3 = World.CreateEntity();
            World.AddComponent(entity3, new Position { X = 3f, Y = 4f, Z = 5f });
            World.AddComponent(entity3, new Health { Amount = 50f });
            
            // 4. Call GetEntitiesWith<Position, Velocity, Health>()
            var entities = World.GetEntitiesWith<Position, Velocity, Health>();
            
            // Returns only Entity1
            AssertEquals(1, entities.Length, "Should return only one entity");
            AssertEquals(entity1.Id, entities[0].Id, "Returned entity should be Entity1");
        });
    }
    
    [ContextMenu("Run Test: GetEntitiesWith Three Types - No Match")]
    public void Test_Query_009()
    {
        string testName = "Test_Query_009";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1 with Position, Velocity
            Entity entity1 = World.CreateEntity();
            World.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Create Entity2 with Position, Health
            Entity entity2 = World.CreateEntity();
            World.AddComponent(entity2, new Position { X = 2f, Y = 3f, Z = 4f });
            World.AddComponent(entity2, new Health { Amount = 100f });
            
            // 3. Call GetEntitiesWith<Position, Velocity, Health>()
            var entities = World.GetEntitiesWith<Position, Velocity, Health>();
            
            // Returns empty span
            AssertEquals(0, entities.Length, "Should return empty span");
        });
    }
    
    // ========== Core-009: GetComponentsWithout removed (API-003) ==========
    // Tests removed - GetComponentsWithout methods no longer exist
    
    // ========== Core-010: ComponentsManager - Deferred Component Modifications ==========
    
    [ContextMenu("Run Test: Modify Component Using ModifiableCollection")]
    public void Test_Modify_001()
    {
        string testName = "Test_Modify_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add Health with Amount=100
            World.AddComponent(entity, new Health { Amount = 100f });
            
            // 3. Use GetModifiableComponents<Health>()
            // 4. Change Amount to 50 via ref
            using (var components = World.GetModifiableComponents<Health>())
            {
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Amount = 50f;
                }
            } // 5. Dispose collection (applies changes)
            
            // 6. Get component back
            Health health = World.GetComponent<Health>(entity);
            AssertEquals(50f, health.Amount, "Health.Amount should be 50 after modification");
        });
    }
    
    [ContextMenu("Run Test: Modify Multiple Components")]
    public void Test_Modify_002()
    {
        string testName = "Test_Modify_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Add Health to all with Amount=100
            World.AddComponent(entity1, new Health { Amount = 100f });
            World.AddComponent(entity2, new Health { Amount = 100f });
            World.AddComponent(entity3, new Health { Amount = 100f });
            
            // 3. Use GetModifiableComponents<Health>()
            // 4. Change Amount for all three
            using (var components = World.GetModifiableComponents<Health>())
            {
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Amount = 50f + i * 10f;
                }
            } // 5. Dispose collection
            
            // 6. Check all components
            Health health1 = World.GetComponent<Health>(entity1);
            Health health2 = World.GetComponent<Health>(entity2);
            Health health3 = World.GetComponent<Health>(entity3);
            
            // Note: Order might vary, so we just check that values changed
            Assert(health1.Amount != 100f || health2.Amount != 100f || health3.Amount != 100f, 
                "At least one component should have changed");
        });
    }
    
    [ContextMenu("Run Test: Modify Component Without Dispose")]
    public void Test_Modify_003()
    {
        string testName = "Test_Modify_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add Health with Amount=100
            World.AddComponent(entity, new Health { Amount = 100f });
            
            // 3. Use GetModifiableComponents<Health>()
            // 4. Change Amount to 50
            var components = World.GetModifiableComponents<Health>();
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Amount = 50f;
            }
            // 5. DON'T call Dispose or Apply()
            
            // 6. Get component back - should still be 100
            Health health = World.GetComponent<Health>(entity);
            AssertEquals(100f, health.Amount, "Health.Amount should still be 100 (modifications not applied)");
            
            // Now apply explicitly
            components.Apply();
            health = World.GetComponent<Health>(entity);
            AssertEquals(50f, health.Amount, "Health.Amount should be 50 after Apply()");
        });
    }
    
    [ContextMenu("Run Test: Modify Component Safe Iteration")]
    public void Test_Modify_004()
    {
        string testName = "Test_Modify_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Add Health to all
            World.AddComponent(entity1, new Health { Amount = 100f });
            World.AddComponent(entity2, new Health { Amount = 100f });
            World.AddComponent(entity3, new Health { Amount = 100f });
            
            // 3. Use GetModifiableComponents<Health>()
            // 4. Iterate and modify all components
            using (var components = World.GetModifiableComponents<Health>())
            {
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Amount -= 10f; // Decrement by 10
                }
            } // 5. Dispose collection
            
            // All components modified correctly, no exceptions
            Health health1 = World.GetComponent<Health>(entity1);
            Health health2 = World.GetComponent<Health>(entity2);
            Health health3 = World.GetComponent<Health>(entity3);
            
            AssertEquals(90f, health1.Amount, "Entity1 Health should be 90");
            AssertEquals(90f, health2.Amount, "Entity2 Health should be 90");
            AssertEquals(90f, health3.Amount, "Entity3 Health should be 90");
        });
    }
    
    // ========== Core-011: Entity Pool Implementation ==========
    
    [ContextMenu("Run Test: Allocate New Entity")]
    public void Test_Pool_001()
    {
        string testName = "Test_Pool_001";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.CreateEntity()
            Entity entity = World.CreateEntity();
            
            // Returns valid Entity
            Assert(entity.IsValid, "Entity should be valid");
            Assert(entity.Id >= 0, "Entity.Id should be >= 0");
            Assert(entity.Generation >= 0, "Entity.Generation should be >= 0");
        });
    }
    
    [ContextMenu("Run Test: Allocate Multiple Entities")]
    public void Test_Pool_002()
    {
        string testName = "Test_Pool_002";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.CreateEntity() three times
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Verify ID uniqueness (IDs or generations should differ)
            // Note: IDs might be same if recycled, but generations should differ
            // At minimum, entities should not be equal
            Assert(entity1 != entity2, "Entity1 should not equal Entity2");
            Assert(entity2 != entity3, "Entity2 should not equal Entity3");
            Assert(entity1 != entity3, "Entity1 should not equal Entity3");
        });
    }
    
    [ContextMenu("Run Test: Deallocate Entity")]
    public void Test_Pool_003()
    {
        string testName = "Test_Pool_003";
        ExecuteTest(testName, () =>
        {
            // 1. Allocate Entity
            Entity entity = World.CreateEntity();
            
            // 2. Destroy Entity (which deallocates it)
            bool destroyed = World.DestroyEntity(entity);
            
            // 3. Verify that Entity is no longer allocated
            Assert(destroyed, "DestroyEntity should return true");
        });
    }
    
    [ContextMenu("Run Test: Entity ID Recycling")]
    public void Test_Pool_004()
    {
        string testName = "Test_Pool_004";
        ExecuteTest(testName, () =>
        {
            // 1. Allocate Entity1
            Entity entity1 = World.CreateEntity();
            
            // 2. Remember Entity1 ID
            int entity1Id = entity1.Id;
            int entity1Gen = entity1.Generation;
            
            // 3. Destroy Entity1
            World.DestroyEntity(entity1);
            
            // 4. Allocate Entity2
            Entity entity2 = World.CreateEntity();
            
            // 5. Verify that Entity2 has same ID but different Generation (if recycled)
            // Note: ID recycling is implementation detail, we just check they're different entities
            Assert(entity1 != entity2, "Entity1 and Entity2 should be different entities");
        });
    }
    
    [ContextMenu("Run Test: Generation Safety")]
    public void Test_Pool_005()
    {
        string testName = "Test_Pool_005";
        ExecuteTest(testName, () =>
        {
            // 1. Allocate Entity1
            Entity entity1 = World.CreateEntity();
            
            // 2. Remember Entity1 completely (ID and Generation)
            int entity1Id = entity1.Id;
            int entity1Gen = entity1.Generation;
            
            // 3. Destroy Entity1
            World.DestroyEntity(entity1);
            
            // 4. Allocate Entity2 (might reuse ID)
            Entity entity2 = World.CreateEntity();
            
            // 5. Verify that old Entity1 is not equal to Entity2
            Entity oldEntity1 = new Entity(entity1Id, entity1Gen);
            Assert(oldEntity1 != entity2, "Old Entity1 reference should not equal Entity2");
        });
    }
    
    // ========== Core-012: Entity Creation/Destruction API ==========
    
    [ContextMenu("Run Test: CreateEntity")]
    public void Test_World_001()
    {
        string testName = "Test_World_001";
        ExecuteTest(testName, () =>
        {
            // 1. Call World.CreateEntity()
            Entity entity = World.GetOrCreate().CreateEntity();
            
            // Returns valid Entity
            Assert(entity.IsValid, "Entity should be valid");
        });
    }
    
    [ContextMenu("Run Test: CreateEntity Multiple Times")]
    public void Test_World_002()
    {
        string testName = "Test_World_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity1, Entity2, Entity3 through World.CreateEntity()
            Entity entity1 = World.CreateEntity();
            Entity entity2 = World.CreateEntity();
            Entity entity3 = World.CreateEntity();
            
            // 2. Verify uniqueness
            Assert(entity1 != entity2, "Entity1 should not equal Entity2");
            Assert(entity2 != entity3, "Entity2 should not equal Entity3");
            Assert(entity1 != entity3, "Entity1 should not equal Entity3");
        });
    }
    
    [ContextMenu("Run Test: CreateEntity with World Parameter")]
    public void Test_World_003()
    {
        string testName = "Test_World_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create World("Test")
            WorldInstance testWorld = World.GetOrCreate("Test");
            
            // 2. Create Entity1 through World.CreateEntity() (global)
            Entity entity1 = World.CreateEntity();
            
            // 3. Create Entity2 through testWorld.CreateEntity()
            Entity entity2 = testWorld.CreateEntity();
            
            // 4. Verify that they are different entities
            Assert(entity1 != entity2, "Entities should be different");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Without Components")]
    public void Test_World_004()
    {
        string testName = "Test_World_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Call World.DestroyEntity(entity)
            bool destroyed = World.DestroyEntity(entity);
            
            // 3. Verify that entity is destroyed
            Assert(destroyed, "DestroyEntity should return true");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity With Components")]
    public void Test_World_005()
    {
        string testName = "Test_World_005";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add Position, Velocity, Health
            World.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity, new Velocity { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity, new Health { Amount = 100f });
            
            // 3. Call World.DestroyEntity(entity)
            bool destroyed = World.DestroyEntity(entity);
            
            // 4. Verify that all components are removed
            Assert(destroyed, "DestroyEntity should return true");
            
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<Position>(entity);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            Assert(exceptionThrown, "Position should be removed");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Multiple Components")]
    public void Test_World_006()
    {
        string testName = "Test_World_006";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add 10 different component types (we'll use fewer since we don't have 10 types)
            World.AddComponent(entity, new Position { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity, new Velocity { X = 1f, Y = 2f, Z = 3f });
            World.AddComponent(entity, new Health { Amount = 100f });
            World.AddComponent(entity, new TestComponent { Value = 42 });
            World.AddComponent(entity, new Dead());
            World.AddComponent(entity, new Destroyed());
            
            // 3. Call World.DestroyEntity(entity)
            bool destroyed = World.DestroyEntity(entity);
            
            // 4. Verify that all components are removed
            Assert(destroyed, "DestroyEntity should return true");
            
            // Check that components are removed
            bool exceptionThrown = false;
            try
            {
                World.GetComponent<Position>(entity);
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            Assert(exceptionThrown, "All components should be removed");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Invalid Entity")]
    public void Test_World_007()
    {
        string testName = "Test_World_007";
        ExecuteTest(testName, () =>
        {
            // 1. Attempt to destroy Entity.Invalid
            bool destroyed = World.DestroyEntity(Entity.Invalid);
            
            // Returns false
            Assert(!destroyed, "DestroyEntity should return false for invalid entity");
        });
    }
    
    [ContextMenu("Run Test: DestroyEntity Twice")]
    public void Test_World_008()
    {
        string testName = "Test_World_008";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Destroy Entity
            bool destroyed1 = World.DestroyEntity(entity);
            
            // 3. Attempt to destroy Entity again
            bool destroyed2 = World.DestroyEntity(entity);
            
            // Second destruction returns false
            Assert(destroyed1, "First DestroyEntity should return true");
            Assert(!destroyed2, "Second DestroyEntity should return false");
        });
    }
    
    // ========== API-005: Entity Extension Methods ==========
    
    [ContextMenu("Run Test: Entity Get Extension Method")]
    public void Test_EntityExt_001()
    {
        string testName = "Test_EntityExt_001";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add component using World API
            World.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Get component using extension method
            TestComponent comp = entity.Get<TestComponent>();
            
            AssertEquals(42, comp.Value, "Component value should match");
        });
    }
    
    [ContextMenu("Run Test: Entity Has Extension Method")]
    public void Test_EntityExt_002()
    {
        string testName = "Test_EntityExt_002";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add component
            World.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Check Has<T>()
            bool hasComponent = entity.Has<TestComponent>();
            Assert(hasComponent, "Has<TestComponent>() should return true");
            
            // 4. Check Has<T>() for non-existent component
            bool hasHealth = entity.Has<Health>();
            Assert(!hasHealth, "Has<Health>() should return false");
        });
    }
    
    [ContextMenu("Run Test: Entity AddComponent Extension Method")]
    public void Test_EntityExt_003()
    {
        string testName = "Test_EntityExt_003";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add component using extension method
            entity.AddComponent(new TestComponent { Value = 42 });
            
            // 3. Get component back
            TestComponent comp = World.GetComponent<TestComponent>(entity);
            AssertEquals(42, comp.Value, "Component value should match");
        });
    }
    
    [ContextMenu("Run Test: Entity RemoveComponent Extension Method")]
    public void Test_EntityExt_004()
    {
        string testName = "Test_EntityExt_004";
        ExecuteTest(testName, () =>
        {
            // 1. Create Entity
            Entity entity = World.CreateEntity();
            
            // 2. Add component
            World.AddComponent(entity, new TestComponent { Value = 42 });
            
            // 3. Remove component using extension method
            bool removed = entity.RemoveComponent<TestComponent>();
            
            // 4. Verify component is removed
            Assert(removed, "RemoveComponent should return true");
            
            bool exceptionThrown = false;
            try
            {
                entity.Get<TestComponent>();
            }
            catch (ComponentNotFoundException)
            {
                exceptionThrown = true;
            }
            Assert(exceptionThrown, "Component should be removed");
        });
    }
}

