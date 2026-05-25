using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockPickup : MonoBehaviour
{
    private int slotIndex = 9;

    public void Init(int index)
    {
        slotIndex = index;
    }


    private void OnTriggerEnter(Collider other)
    {

        if (slotIndex < 0) return;

        if (other.CompareTag("Rock"))
        {
            ItemManager.Instance?.TryCollectRock(other.gameObject);
            Debug.Log("Ã¤±¼");
        }
           
    }
}
