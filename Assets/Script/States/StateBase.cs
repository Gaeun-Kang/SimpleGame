using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase 
{
    protected Player player;
    public StateBase(Player player)
    { this.player = player; }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void UpdateState();
}
