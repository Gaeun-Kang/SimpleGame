using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{

    //Plaer : Player State 변경 함수 , Player Event, Player Input

    [SerializeField] private GameManager gameManager;

    [Header("input 관련")]
    [SerializeField] private float MoveSpeed = 5.0f;
    [SerializeField] private FixedJoystick joystick;


    [SerializeField] private GameObject SpawnAxe;
    [SerializeField] private Transform Handpoint;

    private Vector3 targetPos;
    private Vector3 dir;
    public bool isMoving;

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

    private void FixedUpdate()
    {
        Walking();
    }

    public void Walking()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * joystick.Vertical + right * joystick.Horizontal;

        // direction.magnitude가 0보다 클 때만 이동 및 회전 처리
        if (direction.magnitude > 0.01f)
        {
            transform.Translate(direction * MoveSpeed * Time.fixedDeltaTime, Space.World);
            transform.rotation = Quaternion.LookRotation(direction);
        }

    }

    public GameObject SpawnAxeAtHand()
    {
        GameObject Spawnobject = Instantiate(SpawnAxe, Handpoint.position, Quaternion.identity);
        return Spawnobject;
    }

    //Event

    public event Action OnEnterMine;
    public event Action OnWorkStation;

    public void TriggerMineEvent()
    {
        OnEnterMine?.Invoke();
    }

    public void TriggerWorkStation()
    {
        OnWorkStation?.Invoke();
    }

}
