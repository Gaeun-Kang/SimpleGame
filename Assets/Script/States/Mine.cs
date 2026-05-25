using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : StateBase
{
    public Mine(Player player) : base(player)
    {

    }
    public override void Enter()
    {
        Debug.Log("Mine State");
    }

    public override void Exit()
    {
       
    }

    public override void UpdateState()
    {
        player.Walking();


        //채굴 애니메이션 재생 

    }
}
