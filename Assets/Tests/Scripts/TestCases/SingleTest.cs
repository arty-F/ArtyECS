using ArtyECS.Core;
using UnityEngine;


public class SingleTest : MonoBehaviour
{
    private void Awake()
    {
        var player = World.CreateEntity();
        player.Add<Player>();
    }
}

