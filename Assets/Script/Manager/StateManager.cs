using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//StateManager : 상태 전환 함수 관리.
//시간 문제상 미구현. Player.cs에서 전담 

public class StateManager : MonoBehaviour
{

    /*
   public static StateManager Instance { get; private set; }

    [SerializeField] private Player player;
    private StateBase curState;

    void Awake()
    {
        if(player == null)
        player = GetComponent<Player>();
    }

    void Start()
    {
        ChangeState(new Idle(player));
    }

    void Update()
    {
        curState?.UpdateState();
    }


    public void ChangeState(StateBase nextState)
    {
        curState?.Exit();
        curState = nextState;
        curState.Enter();
    }
    */
}
