using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField] private PlayerState curState;
    [SerializeField] private StateBase stateBase;


    private void Awake()
    {
        if(stateBase == null)
        { print("StateBase가 없습니다"); }

        //첫 시작에는 항상 Idle
        curState = PlayerState.Idle;
    }

    public void changeState(PlayerState nextState)
    {
        if (curState == nextState) return;

        stateBase.Exit();
        curState = nextState;
        stateBase.Enter();
    }

    void Update()
    {
     
    }
}
