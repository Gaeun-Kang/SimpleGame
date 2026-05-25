using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBag : MonoBehaviour
{

    [SerializeField] private GameObject RockPrefab;
    [SerializeField] private Transform BagPosition;
    [SerializeField] private float stackSpacing = 0.25f;

    private List<GameObject> PlayerRock = new List<GameObject>();
    private Queue<GameObject> RockPool = new Queue<GameObject>();
    private int maxcapacity;

    public bool IsFull => PlayerRock.Count >= maxcapacity;

    //사용할 Rock GameObject pool 사전 생성 
    public void Init(int capacity)
    {
        maxcapacity = capacity;

        for (int i = 0; i < capacity; i++)
        {
            GameObject obj = Instantiate(RockPrefab, transform);
            obj.SetActive(false);
            RockPool.Enqueue(obj);
        }
    }

    //돌 적재 
    public void CarryRock()
    {
        if (IsFull) return;

        GameObject displayRock = GetFromPool();
        int index = PlayerRock.Count;

        displayRock.transform.SetParent(BagPosition);
        displayRock.transform.localPosition = new Vector3(0f, index * stackSpacing, 0f);
        displayRock.transform.localRotation = Quaternion.identity;
        displayRock.SetActive(true);

        PlayerRock.Add(displayRock);

        if (IsFull) { Debug.Log("MAX Rock"); }
    }

    public void minusRock()
    {
        if (RockPool.Count == 0) return;  

        bool wasFull = IsFull;

        int top = PlayerRock.Count - 1;
        GameObject displayRock = PlayerRock[top];
        PlayerRock.RemoveAt(top);
   
        ReturnToPool(displayRock);

    }

    public void Clear()
    {
        foreach (var obj in PlayerRock)
        {
            ReturnToPool(obj);
        }
    }

    private GameObject GetFromPool()
    {
        if (RockPool.Count > 0)
            return RockPool.Dequeue();

        return Instantiate(RockPrefab, transform);
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        RockPool.Enqueue(obj);
    }

}
