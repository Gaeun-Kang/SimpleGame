using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make : StateBase
{
    public Make(Player player) : base(player)
    {
    }

    public override void Enter()
    {
        Debug.Log("Make");
    }

    public override void Exit()
    {
     
    }

    public override void UpdateState()
    {
    }

}
