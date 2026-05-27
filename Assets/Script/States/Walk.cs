using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Walk : StateBase
{

    public Walk(Player player) : base(player)
    {
    }

    public override void Enter()
    {
        Debug.Log("Walk State");
        player.OnEnterMine += EntertoMine;
    }

    public override void Exit()
    {
        player.OnEnterMine -= EntertoMine;
    }

    public override void UpdateState()
    {
        player.HandleInput();
        if (player.isMoving == true)
        {
            player.Walking();
        }       
    }

    private void EntertoMine()
    {
        player.ChangeState(new Mine(player));
    }

    private void EntertoWorkStation()
    {

    }


}
