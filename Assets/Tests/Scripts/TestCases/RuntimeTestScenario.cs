#if UNITY_EDITOR
using UnityEngine;
using ArtyECS.Core;

public class RuntimeTestScenario : MonoBehaviour
{
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
}
#endif

