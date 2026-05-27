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
        player.OnWorkStation += EntertoWorkStation;
    }

    public override void Exit()
    {
        player.OnEnterMine -= EntertoMine;
        player.OnWorkStation -= EntertoWorkStation;
    }

    public override void UpdateState()
    {

    }


    private void EntertoMine()
    {
        player.ChangeState(new Mine(player));
    }

    private void EntertoWorkStation()
    {
        player.ChangeState(new Make(player));
    }


}
