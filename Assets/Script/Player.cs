using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private float MoveSpeed = 5.0f;

    private StateBase curState;

    public Rigidbody rb;
    public Collider playercol;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playercol = GetComponentInChildren<Collider>();
        ChangeState(new Idle(this));
    }

    public void ChangeState(StateBase nextState)
    {
        curState?.Exit();

        curState = nextState;

        curState.Enter();
    }

    private void Update()
    {
        curState?.UpdateState();

    }

    public void Walking()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(x, 0f, z).normalized;
        rb.MovePosition(rb.position + moveDir * MoveSpeed * Time.deltaTime);
    }

    //Event

    public event Action OnEnterMine;
    public event Action OnWorkStation;

    public void TriggerMineEvent()
    {
        OnEnterMine?.Invoke();
    }


}
