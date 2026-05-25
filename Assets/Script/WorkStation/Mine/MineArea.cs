using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineArea : MonoBehaviour, PlayerEnterInterface
{

    private Player player;

    void Awake()
    {
        //씬에 있는 player 할당 
        player = FindObjectOfType<Player>();
    }

    public void OnPlayerEnter(Player player)
    {
       // Debug.Log("Player Mine 입장");
        player.TriggerMineEvent();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        OnPlayerEnter(player);
    }


}
