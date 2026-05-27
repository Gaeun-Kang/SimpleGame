using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : StateBase
{
    private GameObject MiningAxe;
    public Mine(Player player) : base(player)
    {

    }
    public override void Enter()
    {
        Debug.Log("Mine State");
        MiningAxe = player.SpawnAxeAtHand();
    }

    public override void Exit()
    {
        if (MiningAxe != null)
        {

            MiningAxe.SetActive(false);
        }
    }

    public override void UpdateState()
    {
   
   
       //layer.Walking();


        //채굴 애니메이션 재생 


        // player.ChangeState(new Make(player));
    }
}
