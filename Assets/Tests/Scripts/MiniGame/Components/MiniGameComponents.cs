using ArtyECS.Core;
using System;
using UnityEngine;

public class Player : Context { }

public class Enemy : Context { }

public class Config : Context { }

[Serializable]
public class PlayerSpawnConfig : Context
{
    public GameObject Prefab;
    public float PlayerBaseSpeed = 4f;
    public float PowerupCollectionRadius = 1f;
    public float Health = 100f;
}

[Serializable]
public class EnemySpawnConfig : Context
{
    public GameObject Prefab;
    public float EnemySpeed = 1f;
    public float SpawnPeriod = 0.5f;
    public int EnemiesPerSpawn = 500;
    public int MaxEnemies = 5000;
    public float SpawnMaxDistance = 50;
    public float SpawnMinDistance = 30;
    public float ExplosionTriggerDistane = 1.5f;
    public float ExplodeTime = 1f;
    public float ExplodeRadius = 2f;
}

[Serializable]
public class CollectableSpawnConfig : Context
{
    public GameObject Prefab;
    public int MaxCollectables = 20;
    public float SpawnMinRange = 15;
    public float SpawnMaxRange = 30;
    public float SpeedBonus = 1f;
    public float BonusDuration = 6f;
    public float SpawnPeriod = 1f;
}

public class Position : Context 
{
    public float X;
    public float Y;
    public float Z;
}

public class Health : Context
{
    public float Amount;
}

public class MoveDirection : Context
{
    public float X;
    public float Y;
    public float Z;
}

public class Speed : Context
{
    public float Value;
}

public class ProximityBomb : Context 
{
    public float TriggerDistance;
}

public class Explosion : Context
{
    public float TimeRemaining;
    public float ExplosionRadius;
}

public class Destroying : Context { } 

public class Collectable : Context 
{
    public float SpeedBonus;
    public float BonusDuration;
}

public class CollectablePickuper : Context 
{
    public float PickupRange;
}

public class SpeedBonus : Context
{
    public float Value;
    public float TimeRamaining;
}