using UnityEngine;
using ArtyECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;

public class QueryTests : TestBase
{
    /*
    [ContextMenu(nameof(Test_Query_With_Single))]
    public void Test_Query_With_Single()
    {
        string testName = "Test_Query_With_Single";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var entities = World.Query().With<Position>().Execute().ToList();

            AssertEquals(1, entities.Count, "Should return one entity with Position");
            AssertEquals(entity1.Id, entities[0].Id, "Returned entity should be entity1");
        });
    }

    [ContextMenu(nameof(Test_Query_With_Multiple))]
    public void Test_Query_With_Multiple()
    {
        string testName = "Test_Query_With_Multiple";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Velocity { X = 2f, Y = 3f, Z = 4f });

            var entities = World.Query().With<Position>().With<Velocity>().Execute().ToList();

            AssertEquals(1, entities.Count, "Should return one entity with both Position and Velocity");
            AssertEquals(entity1.Id, entities[0].Id, "Returned entity should be entity1");
        });
    }

    [ContextMenu(nameof(Test_Query_With_Three))]
    public void Test_Query_With_Three()
    {
        string testName = "Test_Query_With_Three";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Health { Amount = 100f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Velocity { X = 2f, Y = 3f, Z = 4f });

            var entities = World.Query().With<Position>().With<Velocity>().With<Health>().Execute().ToList();

            AssertEquals(1, entities.Count, "Should return one entity with Position, Velocity, and Health");
            AssertEquals(entity1.Id, entities[0].Id, "Returned entity should be entity1");
        });
    }

    [ContextMenu(nameof(Test_Query_With_Empty))]
    public void Test_Query_With_Empty()
    {
        string testName = "Test_Query_With_Empty";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });

            var entities = World.Query().Execute().ToList();

            AssertEquals(1, entities.Count, "Query without With should return all entities");
        });
    }

    [ContextMenu(nameof(Test_Query_With_NoMatch))]
    public void Test_Query_With_NoMatch()
    {
        string testName = "Test_Query_With_NoMatch";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var entities = World.Query().With<Health>().Execute().ToList();

            AssertEquals(0, entities.Count, "Should return empty when no entities match");
        });
    }

    [ContextMenu(nameof(Test_Query_Without_Single))]
    public void Test_Query_Without_Single()
    {
        string testName = "Test_Query_Without_Single";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Dead());

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var entities = World.Query().With<Position>().Without<Dead>().Execute().ToList();

            AssertEquals(1, entities.Count, "Should return one entity with Position but without Dead");
            AssertEquals(entity2.Id, entities[0].Id, "Returned entity should be entity2");
        });
    }

    [ContextMenu(nameof(Test_Query_Without_Multiple))]
    public void Test_Query_Without_Multiple()
    {
        string testName = "Test_Query_Without_Multiple";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Dead());

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Destroyed());

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Position { X = 3f, Y = 4f, Z = 5f });

            var entity4 = World.CreateEntity();
            entity4.AddComponent(new Position { X = 4f, Y = 5f, Z = 6f });
            entity4.AddComponent(new Dead());
            entity4.AddComponent(new Destroyed());

            var entities = World.Query().With<Position>().Without<Dead>().Without<Destroyed>().Execute().ToList();

            AssertEquals(1, entities.Count, "Should return one entity with Position but without Dead or Destroyed");
            AssertEquals(entity3.Id, entities[0].Id, "Returned entity should be entity3");
        });
    }

    [ContextMenu(nameof(Test_Query_Without_Only))]
    public void Test_Query_Without_Only()
    {
        string testName = "Test_Query_Without_Only";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Dead());

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Health { Amount = 100f });
            entity3.AddComponent(new Dead());

            var entities = World.Query().Without<Dead>().Execute().ToList();

            AssertEquals(1, entities.Count, "Should return one entity without Dead");
            AssertEquals(entity2.Id, entities[0].Id, "Returned entity should be entity2");
        });
    }

    [ContextMenu(nameof(Test_Query_WithWithout_Complex))]
    public void Test_Query_WithWithout_Complex()
    {
        string testName = "Test_Query_WithWithout_Complex";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Health { Amount = 100f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Velocity { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Dead());

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Position { X = 3f, Y = 4f, Z = 5f });
            entity3.AddComponent(new Velocity { X = 3f, Y = 4f, Z = 5f });

            var entity4 = World.CreateEntity();
            entity4.AddComponent(new Position { X = 4f, Y = 5f, Z = 6f });
            entity4.AddComponent(new Health { Amount = 50f });

            var entities = World.Query()
                .With<Position>()
                .With<Velocity>()
                .Without<Dead>()
                .Execute()
                .ToList();

            AssertEquals(2, entities.Count, "Should return two entities with Position and Velocity but without Dead");
            
            var entityIds = entities.Select(e => e.Id).ToList();
            Assert(entityIds.Contains(entity1.Id), "Should include entity1");
            Assert(entityIds.Contains(entity3.Id), "Should include entity3");
            Assert(!entityIds.Contains(entity2.Id), "Should not include entity2 (has Dead)");
            Assert(!entityIds.Contains(entity4.Id), "Should not include entity4 (no Velocity)");
        });
    }

    [ContextMenu(nameof(Test_Query_WithWithout_AllHaveExcluded))]
    public void Test_Query_WithWithout_AllHaveExcluded()
    {
        string testName = "Test_Query_WithWithout_AllHaveExcluded";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Dead());

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Dead());

            var entities = World.Query().With<Position>().Without<Dead>().Execute().ToList();

            AssertEquals(0, entities.Count, "Should return empty when all entities have excluded component");
        });
    }

    [ContextMenu(nameof(Test_Query_MultipleMatches))]
    public void Test_Query_MultipleMatches()
    {
        string testName = "Test_Query_MultipleMatches";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Velocity { X = 2f, Y = 3f, Z = 4f });

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Position { X = 3f, Y = 4f, Z = 5f });
            entity3.AddComponent(new Velocity { X = 3f, Y = 4f, Z = 5f });

            var entity4 = World.CreateEntity();
            entity4.AddComponent(new Position { X = 4f, Y = 5f, Z = 6f });

            var entities = World.Query().With<Position>().With<Velocity>().Execute().ToList();

            AssertEquals(3, entities.Count, "Should return three entities with Position and Velocity");
        });
    }

    [ContextMenu(nameof(Test_Query_WithAdditionalComponents))]
    public void Test_Query_WithAdditionalComponents()
    {
        string testName = "Test_Query_WithAdditionalComponents";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Health { Amount = 100f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Velocity { X = 2f, Y = 3f, Z = 4f });

            var entities = World.Query().With<Position>().With<Velocity>().Execute().ToList();

            AssertEquals(2, entities.Count, "Should return entities with Position and Velocity, even if they have additional components");
        });
    }

    [ContextMenu(nameof(Test_Query_WorldInstance))]
    public void Test_Query_WorldInstance()
    {
        string testName = "Test_Query_WorldInstance";
        ExecuteTest(testName, () =>
        {
            var world1 = World.GetOrCreate("TestWorld1");
            var world2 = World.GetOrCreate("TestWorld2");

            var entity1 = world1.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });

            var entity2 = world2.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });

            var entities1 = world1.Query().With<Position>().Execute().ToList();
            var entities2 = world2.Query().With<Position>().Execute().ToList();

            AssertEquals(1, entities1.Count, "World1 should return one entity");
            AssertEquals(1, entities2.Count, "World2 should return one entity");
            AssertEquals(entity1.Id, entities1[0].Id, "World1 entity should be entity1");
            AssertEquals(entity2.Id, entities2[0].Id, "World2 entity should be entity2");
        });
    }

    [ContextMenu(nameof(Test_Query_ReuseBuilder))]
    public void Test_Query_ReuseBuilder()
    {
        string testName = "Test_Query_ReuseBuilder";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var queryBuilder = World.Query();
            
            var entities1 = queryBuilder.With<Position>().Execute().ToList();
            AssertEquals(1, entities1.Count, "First query should return one entity");

            var queryBuilder2 = World.Query();
            var entities2 = queryBuilder2.With<Velocity>().Execute().ToList();
            
            AssertEquals(1, entities2.Count, "Second query should return one entity");
            AssertEquals(entity2.Id, entities2[0].Id, "Second query should return entity2");
        });
    }

    [ContextMenu(nameof(Test_Query_EmptyWorld))]
    public void Test_Query_EmptyWorld()
    {
        string testName = "Test_Query_EmptyWorld";
        ExecuteTest(testName, () =>
        {
            var entities = World.Query().With<Position>().Execute().ToList();

            AssertEquals(0, entities.Count, "Should return empty when world is empty");
        });
    }

    [ContextMenu(nameof(Test_Query_With_MoreThanThree))]
    public void Test_Query_With_MoreThanThree()
    {
        string testName = "Test_Query_With_MoreThanThree";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Health { Amount = 100f });
            entity1.AddComponent(new TestComponent { Value = 42 });

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Velocity { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Health { Amount = 50f });

            var entities = World.Query()
                .With<Position>()
                .With<Velocity>()
                .With<Health>()
                .With<TestComponent>()
                .Execute()
                .ToList();

            AssertEquals(1, entities.Count, "Should return one entity with all four components");
            AssertEquals(entity1.Id, entities[0].Id, "Returned entity should be entity1");
        });
    }

    [ContextMenu(nameof(Test_Query_Without_NoWith))]
    public void Test_Query_Without_NoWith()
    {
        string testName = "Test_Query_Without_NoWith";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Dead());

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });

            var entity3 = World.CreateEntity();
            entity3.AddComponent(new Health { Amount = 100f });
            entity3.AddComponent(new Dead());

            var entities = World.Query().Without<Dead>().Execute().ToList();

            AssertEquals(1, entities.Count, "Should return entities without Dead component");
            AssertEquals(entity2.Id, entities[0].Id, "Returned entity should be entity2");
        });
    }

    [ContextMenu(nameof(Test_Query_Chaining))]
    public void Test_Query_Chaining()
    {
        string testName = "Test_Query_Chaining";
        ExecuteTest(testName, () =>
        {
            var entity1 = World.CreateEntity();
            entity1.AddComponent(new Position { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Velocity { X = 1f, Y = 2f, Z = 3f });
            entity1.AddComponent(new Health { Amount = 100f });
            entity1.AddComponent(new Dead());

            var entity2 = World.CreateEntity();
            entity2.AddComponent(new Position { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Velocity { X = 2f, Y = 3f, Z = 4f });
            entity2.AddComponent(new Health { Amount = 50f });

            var entities = World.Query()
                .With<Position>()
                .With<Velocity>()
                .With<Health>()
                .Without<Dead>()
                .Execute()
                .ToList();

            AssertEquals(1, entities.Count, "Should return one entity matching all conditions");
            AssertEquals(entity2.Id, entities[0].Id, "Returned entity should be entity2");
        });
    }*/
}

