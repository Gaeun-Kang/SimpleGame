using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handcuffpickup : MonoBehaviour
{

    private ItemManager itemManager;

    public void Init(ItemManager manager)
    {
        itemManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if(itemManager == null) return;

        itemManager.TryCollectHandcuff(gameObject);
    }

}
