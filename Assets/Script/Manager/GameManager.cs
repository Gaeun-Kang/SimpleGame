using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    //다른 매니저 접근 용도 (Singleton)

    public static GameManager gameManger;
    public UIManager UiManager { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public ItemManager ItemManager { get; private set; }

    public StateManager StateManager { get; private set; }


    protected void Awake()
    {
        Init();
    }

    public void Init()
    {
        UiManager = FindObjectOfType<UIManager>();
        SoundManager = FindObjectOfType<SoundManager>();
        ItemManager = FindObjectOfType<ItemManager>();
        StateManager = FindObjectOfType<StateManager>();
    }
}
