using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//돌 -> 수갑 
public class WorkStation : MonoBehaviour
{
    private Coroutine workRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;


        if (workRoutine == null)
        {
            Debug.Log("작성중");

        }
    }

    public void DepositRockFromWorker()
    {

    }

}
