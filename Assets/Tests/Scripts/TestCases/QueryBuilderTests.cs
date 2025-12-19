using UnityEngine;
using ArtyECS.Core;
using System;
using System.Linq;

/// <summary>
/// Test class for QueryBuilder functionality (API-014).
/// </summary>
public class QueryBuilderTests : TestBase
{
    private bool ContainsEntity(ReadOnlySpan<Entity> span, Entity entity)
    {
        foreach (var e in span)
        {
            if (e.Id == entity.Id && e.Generation == entity.Generation)
            {
                return true;
            }
        }
        return false;
    }
    // ========== API-014: Query Builder Pattern ==========
    
    [ContextMenu("Run Test: Single With Condition Returns Correct Entities")]
    public void Test_Query_001()
    {
        string testName = "Test_Query_001";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities with different components
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity3, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Query entities with Position component
            var entities = world.Query().With<Position>().Execute();
            
            // 3. Verify results
            AssertEquals(2, entities.Length, "Should return 2 entities with Position");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
            Assert(ContainsEntity(entities, entity2), "Result should contain entity2");
            Assert(!ContainsEntity(entities, entity3), "Result should not contain entity3");
        });
    }
    
    [ContextMenu("Run Test: Multiple With Conditions Returns Intersection")]
    public void Test_Query_002()
    {
        string testName = "Test_Query_002";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities with different component combinations
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            Entity entity4 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Velocity { X = 4f, Y = 5f, Z = 6f });
            
            world.AddComponent(entity3, new Position { X = 7f, Y = 8f, Z = 9f });
            
            world.AddComponent(entity4, new Velocity { X = 10f, Y = 11f, Z = 12f });
            
            // 2. Query entities with Position AND Velocity
            var entities = world.Query().With<Position>().With<Velocity>().Execute();
            
            // 3. Verify results (only entities with both components)
            AssertEquals(2, entities.Length, "Should return 2 entities with both Position and Velocity");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
            Assert(ContainsEntity(entities, entity2), "Result should contain entity2");
            Assert(!ContainsEntity(entities, entity3), "Result should not contain entity3");
            Assert(!ContainsEntity(entities, entity4), "Result should not contain entity4");
        });
    }
    
    [ContextMenu("Run Test: Single Without Condition Returns Correct Entities")]
    public void Test_Query_003()
    {
        string testName = "Test_Query_003";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities with different components
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Dead());
            world.AddComponent(entity3, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Query entities without Dead component
            var entities = world.Query().Without<Dead>().Execute();
            
            // 3. Verify results (all entities except entity2)
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
            Assert(!ContainsEntity(entities, entity2), "Result should not contain entity2 (has Dead)");
            Assert(ContainsEntity(entities, entity3), "Result should contain entity3");
        });
    }
    
    [ContextMenu("Run Test: Multiple Without Conditions Returns Entities Without Any")]
    public void Test_Query_004()
    {
        string testName = "Test_Query_004";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities with different components
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            Entity entity4 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Dead());
            
            world.AddComponent(entity3, new Velocity { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity3, new Destroyed());
            
            world.AddComponent(entity4, new Health { Amount = 100f });
            
            // 2. Query entities without Dead AND without Destroyed
            var entities = world.Query().Without<Dead>().Without<Destroyed>().Execute();
            
            // 3. Verify results (entities without Dead and without Destroyed)
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
            Assert(!ContainsEntity(entities, entity2), "Result should not contain entity2 (has Dead)");
            Assert(!ContainsEntity(entities, entity3), "Result should not contain entity3 (has Destroyed)");
            Assert(ContainsEntity(entities, entity4), "Result should contain entity4");
        });
    }
    
    [ContextMenu("Run Test: Combination of With and Without Conditions")]
    public void Test_Query_005()
    {
        string testName = "Test_Query_005";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities with different component combinations
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            Entity entity4 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Velocity { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Dead());
            
            world.AddComponent(entity3, new Position { X = 7f, Y = 8f, Z = 9f });
            world.AddComponent(entity3, new Velocity { X = 7f, Y = 8f, Z = 9f });
            
            world.AddComponent(entity4, new Position { X = 10f, Y = 11f, Z = 12f });
            
            // 2. Query entities with Position AND Velocity, but WITHOUT Dead
            var entities = world.Query().With<Position>().With<Velocity>().Without<Dead>().Execute();
            
            // 3. Verify results (entities with both Position and Velocity, but not Dead)
            AssertEquals(2, entities.Length, "Should return 2 entities");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
            Assert(!ContainsEntity(entities, entity2), "Result should not contain entity2 (has Dead)");
            Assert(ContainsEntity(entities, entity3), "Result should contain entity3");
            Assert(!ContainsEntity(entities, entity4), "Result should not contain entity4 (no Velocity)");
        });
    }
    
    [ContextMenu("Run Test: Empty Query Returns Empty Span")]
    public void Test_Query_006()
    {
        string testName = "Test_Query_006";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create some entities
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Query with no conditions (empty query)
            var entities = world.Query().Execute();
            
            // 3. Verify empty result
            AssertEquals(0, entities.Length, "Empty query should return empty span");
        });
    }
    
    [ContextMenu("Run Test: Empty World Returns Empty Span")]
    public void Test_Query_007()
    {
        string testName = "Test_Query_007";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Don't create any entities
            
            // 2. Query entities with Position
            var entities = world.Query().With<Position>().Execute();
            
            // 3. Verify empty result
            AssertEquals(0, entities.Length, "Empty world should return empty span");
        });
    }
    
    [ContextMenu("Run Test: No Matching Entities Returns Empty Span")]
    public void Test_Query_008()
    {
        string testName = "Test_Query_008";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities without Position component
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Health { Amount = 100f });
            
            // 2. Query entities with Position (none exist)
            var entities = world.Query().With<Position>().Execute();
            
            // 3. Verify empty result
            AssertEquals(0, entities.Length, "No matching entities should return empty span");
        });
    }
    
    [ContextMenu("Run Test: All Entities Have Exclusion Components Returns Empty Span")]
    public void Test_Query_009()
    {
        string testName = "Test_Query_009";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities, all with Dead component
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Dead());
            
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Dead());
            
            world.AddComponent(entity3, new Health { Amount = 100f });
            world.AddComponent(entity3, new Dead());
            
            // 2. Query entities without Dead (all have Dead)
            var entities = world.Query().Without<Dead>().Execute();
            
            // 3. Verify empty result
            AssertEquals(0, entities.Length, "All entities excluded should return empty span");
        });
    }
    
    [ContextMenu("Run Test: WorldInstance Query Method")]
    public void Test_Query_010()
    {
        string testName = "Test_Query_010";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Use instance method world.Query()
            var entities = world.Query().With<Position>().Execute();
            
            // 3. Verify results
            AssertEquals(1, entities.Length, "Should return 1 entity with Position");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
        });
    }
    
    [ContextMenu("Run Test: Static World Query Method")]
    public void Test_Query_011()
    {
        string testName = "Test_Query_011";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Use static method World.Query()
            var entities = World.Query().With<Position>().Execute();
            
            // 3. Verify results
            AssertEquals(1, entities.Length, "Should return 1 entity with Position");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
        });
    }
    
    [ContextMenu("Run Test: Idempotent Behavior Multiple With Same Type")]
    public void Test_Query_012()
    {
        string testName = "Test_Query_012";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Query with multiple With<Position>() calls (idempotent)
            var entities = world.Query().With<Position>().With<Position>().With<Position>().Execute();
            
            // 3. Verify results (should be same as single With<Position>())
            AssertEquals(1, entities.Length, "Should return 1 entity with Position");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
        });
    }
    
    [ContextMenu("Run Test: Idempotent Behavior Multiple Without Same Type")]
    public void Test_Query_013()
    {
        string testName = "Test_Query_013";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Dead());
            
            // 2. Query with multiple Without<Dead>() calls (idempotent)
            var entities = world.Query().Without<Dead>().Without<Dead>().Without<Dead>().Execute();
            
            // 3. Verify results (should be same as single Without<Dead>())
            AssertEquals(1, entities.Length, "Should return 1 entity without Dead");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
        });
    }
    
    [ContextMenu("Run Test: Entities With Multiple Components")]
    public void Test_Query_014()
    {
        string testName = "Test_Query_014";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entity with multiple components
            Entity entity1 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Health { Amount = 100f });
            
            // 2. Query with multiple WITH conditions
            var entities = world.Query().With<Position>().With<Velocity>().With<Health>().Execute();
            
            // 3. Verify results
            AssertEquals(1, entities.Length, "Should return 1 entity with all components");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
            
            // 4. Verify entity has all components
            Position pos = world.GetComponent<Position>(entity1);
            Velocity vel = world.GetComponent<Velocity>(entity1);
            Health health = world.GetComponent<Health>(entity1);
            
            AssertEquals(1f, pos.X, "Position.X should be 1f");
            AssertEquals(1f, vel.X, "Velocity.X should be 1f");
            AssertEquals(100f, health.Amount, "Health.Amount should be 100f");
        });
    }
    
    [ContextMenu("Run Test: Query Builder Struct Copying")]
    public void Test_Query_015()
    {
        string testName = "Test_Query_015";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Create query builder and copy it
            var builder1 = world.Query().With<Position>();
            var builder2 = builder1; // Struct copy
            
            // 3. Execute both builders
            var entities1 = builder1.Execute();
            var entities2 = builder2.Execute();
            
            // 4. Verify both return same results
            AssertEquals(entities1.Length, entities2.Length, "Both builders should return same count");
            AssertEquals(1, entities1.Length, "Should return 1 entity");
            Assert(ContainsEntity(entities1, entity1), "Result should contain entity1");
            Assert(ContainsEntity(entities2, entity1), "Copied builder should also contain entity1");
        });
    }
    
    [ContextMenu("Run Test: Complex Query With Multiple Conditions")]
    public void Test_Query_016()
    {
        string testName = "Test_Query_016";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities with various component combinations
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            Entity entity4 = world.CreateEntity();
            Entity entity5 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Health { Amount = 100f });
            
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Velocity { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Dead());
            
            world.AddComponent(entity3, new Position { X = 7f, Y = 8f, Z = 9f });
            world.AddComponent(entity3, new Velocity { X = 7f, Y = 8f, Z = 9f });
            world.AddComponent(entity3, new Health { Amount = 50f });
            world.AddComponent(entity3, new Destroyed());
            
            world.AddComponent(entity4, new Position { X = 10f, Y = 11f, Z = 12f });
            
            world.AddComponent(entity5, new Velocity { X = 13f, Y = 14f, Z = 15f });
            world.AddComponent(entity5, new Health { Amount = 75f });
            
            // 2. Query: Position AND Velocity AND Health, but WITHOUT Dead AND WITHOUT Destroyed
            var entities = world.Query()
                .With<Position>()
                .With<Velocity>()
                .With<Health>()
                .Without<Dead>()
                .Without<Destroyed>()
                .Execute();
            
            // 3. Verify results (only entity1 matches all conditions)
            AssertEquals(1, entities.Length, "Should return 1 entity matching all conditions");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
            Assert(!ContainsEntity(entities, entity2), "Result should not contain entity2 (has Dead)");
            Assert(!ContainsEntity(entities, entity3), "Result should not contain entity3 (has Destroyed)");
            Assert(!ContainsEntity(entities, entity4), "Result should not contain entity4 (no Velocity, no Health)");
            Assert(!ContainsEntity(entities, entity5), "Result should not contain entity5 (no Position)");
        });
    }
    
    [ContextMenu("Run Test: Query Builder Chaining")]
    public void Test_Query_017()
    {
        string testName = "Test_Query_017";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity1, new Velocity { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity2, new Dead());
            
            // 2. Test fluent chaining
            var entities = world.Query()
                .With<Position>()
                .With<Velocity>()
                .Without<Dead>()
                .Execute();
            
            // 3. Verify results
            AssertEquals(1, entities.Length, "Should return 1 entity");
            Assert(ContainsEntity(entities, entity1), "Result should contain entity1");
        });
    }
    
    [ContextMenu("Run Test: Query With No Components In World")]
    public void Test_Query_018()
    {
        string testName = "Test_Query_018";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities but don't add any components
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            // 2. Query for entities with Position (none have components)
            var entities = world.Query().With<Position>().Execute();
            
            // 3. Verify empty result
            AssertEquals(0, entities.Length, "Should return empty span when no entities have components");
        });
    }
    
    [ContextMenu("Run Test: Query Without When All Entities Have Component")]
    public void Test_Query_019()
    {
        string testName = "Test_Query_019";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities, all with Position
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            Entity entity3 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Position { X = 4f, Y = 5f, Z = 6f });
            world.AddComponent(entity3, new Position { X = 7f, Y = 8f, Z = 9f });
            
            // 2. Query for entities without Position (all have it)
            var entities = world.Query().Without<Position>().Execute();
            
            // 3. Verify empty result
            AssertEquals(0, entities.Length, "Should return empty span when all entities have excluded component");
        });
    }
    
    [ContextMenu("Run Test: Multiple With Types No Intersection")]
    public void Test_Query_020()
    {
        string testName = "Test_Query_020";
        ExecuteTest(testName, () =>
        {
            WorldInstance world = World.GetOrCreate();
            
            // 1. Create entities with different components (no entity has both)
            Entity entity1 = world.CreateEntity();
            Entity entity2 = world.CreateEntity();
            
            world.AddComponent(entity1, new Position { X = 1f, Y = 2f, Z = 3f });
            world.AddComponent(entity2, new Velocity { X = 1f, Y = 2f, Z = 3f });
            
            // 2. Query for entities with Position AND Velocity (no intersection)
            var entities = world.Query().With<Position>().With<Velocity>().Execute();
            
            // 3. Verify empty result
            AssertEquals(0, entities.Length, "Should return empty span when no entities have all WITH components");
        });
    }
}
