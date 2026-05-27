using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Idle : StateBase
{
    //초기 상태 

    public Idle(Player player) : base(player)
    { 
    
    }
    public override void Enter()
    {
        Debug.Log(" Idle State");
    }

    public override void Exit()
    {
        
    }


    public override void UpdateState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            player.ChangeState(new Walk(player));
        }
  
    }
}
