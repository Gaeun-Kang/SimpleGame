using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{

    //Plaer : Player State 변경 함수 , Player Event, Player Input

    [SerializeField] private GameManager gameManager;

    [SerializeField] private float MoveSpeed = 5.0f;
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

    public void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, 100f))
            {
                //Deubg.Log("move");
                targetPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                isMoving = true;
            }

          
        }
    }

    public void Walking()
    {
        Vector3 dir = targetPos - transform.position;

        if (dir.magnitude < 0.05f)
        {
            transform.position = targetPos; // 위치를 목적지에 딱 맞춰줌
            isMoving = false;
            return;
        }

        // 1. 이동 처리 (방향 * 시간 * 속도)
        transform.position += dir.normalized * Time.deltaTime * MoveSpeed;

        // 2. 회전 처리 (Quaternion.Slerp와 Time.deltaTime을 활용해 부드럽게 프레임 독립적 회전)
        if (dir != Vector3.zero)
        {
            Quaternion lookTarget = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookTarget, Time.deltaTime * MoveSpeed);
        }


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
