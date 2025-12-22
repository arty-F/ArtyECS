#if UNITY_EDITOR
using UnityEngine;
using ArtyECS.Core;
using System.Collections.Generic;

public class RuntimeTestScenario : MonoBehaviour
{
    [SerializeField] private string worldName = "Global";
    [SerializeField] private string entityName = "";
    
    private int lastEntityId = -1;
    private int lastEntityGeneration = 0;
    private List<SystemHandler> activeSystems = new List<SystemHandler>();
    
    public string WorldName => worldName;
    public string EntityName => entityName;
    public int LastEntityId => lastEntityId;
    public int LastEntityGeneration => lastEntityGeneration;
    
    private void Start()
    {
        var globalWorld = World.GetOrCreate();
        SetupWorld(globalWorld, "Global", 8);
        
        var localWorld1 = World.GetOrCreate("Local1");
        SetupWorld(localWorld1, "Local1", 5);
        
        var localWorld2 = World.GetOrCreate("Local2");
        SetupWorld(localWorld2, "Local2", 6);
    }
    
    private void SetupWorld(WorldInstance world, string worldName, int entityCount)
    {
        for (int i = 0; i < entityCount; i++)
        {
            var entity = world.CreateEntity();
            
            if (Random.value > 0.5f)
            {
                world.AddComponent(entity, new Position 
                { 
                    X = Random.Range(-10f, 10f), 
                    Y = Random.Range(-10f, 10f), 
                    Z = Random.Range(-10f, 10f) 
                });
                
                if (Random.value > 0.5f)
                {
                    world.AddComponent(entity, new Velocity 
                    { 
                        X = Random.Range(-1f, 1f), 
                        Y = Random.Range(-1f, 1f), 
                        Z = Random.Range(-1f, 1f) 
                    });
                }
            }
            
            if (Random.value > 0.5f)
            {
                world.AddComponent(entity, new Health 
                { 
                    Amount = Random.Range(50f, 100f) 
                });
            }
            
            if (Random.value > 0.5f)
            {
                world.AddComponent(entity, new CounterComponent 
                { 
                    Value = Random.Range(0, 100) 
                });
            }
            
            if (Random.value > 0.3f)
            {
                world.AddComponent(entity, new UpdateCounter 
                { 
                    Value = 0 
                });
            }
            
            if (Random.value > 0.3f)
            {
                world.AddComponent(entity, new FixedUpdateCounter 
                { 
                    Value = 0 
                });
            }
        }
        
        world.AddToUpdate(new MovementSystem());
        world.AddToUpdate(new HealthSystem());
        world.AddToUpdate(new IncrementSystem());
        world.AddToUpdate(new UpdateCounterSystem());
        
        world.AddToFixedUpdate(new FixedUpdateCounterSystem());
    }
    
    public WorldInstance GetWorld()
    {
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            return World.GetOrCreate();
        }
        return World.GetOrCreate(worldName);
    }
    
    public bool CreateWorld(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        
        if (World.Exists(name))
        {
            return false;
        }
        
        World.GetOrCreate(name);
        return true;
    }
    
    public bool DeleteWorld(string name)
    {
        if (string.IsNullOrEmpty(name) || name == "Global")
        {
            return false;
        }
        
        if (!World.Exists(name))
        {
            return false;
        }
        
        var world = World.GetOrCreate(name);
        return World.Destroy(world);
    }
    
    public Entity CreateEntity(string worldName)
    {
        WorldInstance world;
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            world = World.GetOrCreate();
        }
        else
        {
            world = World.GetOrCreate(worldName);
        }
        
        var entity = world.CreateEntity();
        lastEntityId = entity.Id;
        lastEntityGeneration = entity.Generation;
        entityName = $"Entity_{entity.Id}_Gen{entity.Generation}";
        return entity;
    }
    
    public Entity ParseEntity(string entityName)
    {
        if (string.IsNullOrEmpty(entityName))
        {
            return Entity.Invalid;
        }
        
        var parts = entityName.Split('_');
        if (parts.Length < 3)
        {
            return Entity.Invalid;
        }
        
        if (parts[0] != "Entity")
        {
            return Entity.Invalid;
        }
        
        if (!int.TryParse(parts[1], out int id))
        {
            return Entity.Invalid;
        }
        
        if (!parts[2].StartsWith("Gen"))
        {
            return Entity.Invalid;
        }
        
        if (!int.TryParse(parts[2].Substring(3), out int generation))
        {
            return Entity.Invalid;
        }
        
        return new Entity(id, generation);
    }
    
    public bool IsEntityValid(Entity entity, string worldName)
    {
        WorldInstance world;
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            world = World.GetOrCreate();
        }
        else
        {
            world = World.GetOrCreate(worldName);
        }
        
        return world.IsEntityValid(entity);
    }
    
    public void AddComponent<T>(Entity entity, T component, string worldName) where T : struct, IComponent
    {
        WorldInstance world;
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            world = World.GetOrCreate();
        }
        else
        {
            world = World.GetOrCreate(worldName);
        }
        
        world.AddComponent(entity, component);
    }
    
    public bool RemoveComponent<T>(Entity entity, string worldName) where T : struct, IComponent
    {
        WorldInstance world;
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            world = World.GetOrCreate();
        }
        else
        {
            world = World.GetOrCreate(worldName);
        }
        
        return world.RemoveComponent<T>(entity);
    }
    
    public ComponentInfo[] GetEntityComponents(Entity entity, string worldName)
    {
        WorldInstance world;
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            world = World.GetOrCreate();
        }
        else
        {
            world = World.GetOrCreate(worldName);
        }
        
        return world.GetAllComponentInfos(entity);
    }
    
    public SystemHandler AddSystem(SystemHandler system, string worldName, string queue)
    {
        WorldInstance world;
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            world = World.GetOrCreate();
        }
        else
        {
            world = World.GetOrCreate(worldName);
        }
        
        if (queue == "Update")
        {
            world.AddToUpdate(system);
        }
        else if (queue == "FixedUpdate")
        {
            world.AddToFixedUpdate(system);
        }
        
        activeSystems.Add(system);
        return system;
    }
    
    public bool RemoveSystem(SystemHandler system, string worldName, string queue)
    {
        WorldInstance world;
        if (string.IsNullOrEmpty(worldName) || worldName == "Global")
        {
            world = World.GetOrCreate();
        }
        else
        {
            world = World.GetOrCreate(worldName);
        }
        
        bool removed = false;
        if (queue == "Update")
        {
            removed = world.RemoveFromUpdate(system);
        }
        else if (queue == "FixedUpdate")
        {
            removed = world.RemoveFromFixedUpdate(system);
        }
        
        if (removed)
        {
            activeSystems.Remove(system);
        }
        
        return removed;
    }
    
    public IReadOnlyList<SystemHandler> GetActiveSystems()
    {
        return activeSystems;
    }
    
    public void SetWorldName(string name)
    {
        worldName = name;
    }
    
    public void SetEntityName(string name)
    {
        entityName = name;
    }
}
#endif

