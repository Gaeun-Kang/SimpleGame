using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject Logo;
    [SerializeField] private Button SoundBt;
    [SerializeField] private Button PlayBt;

    [Header("2D Ui 위치")]
    [SerializeField] private Sprite WorkInput;
    [SerializeField] private Sprite WorkOutput;


    [Header("3D UI 위치")]
    [SerializeField] private Transform ArrowUIpos;
    [SerializeField] private Transform MaxUIpos;


    void Awake()
    {
       SoundBt = GetComponent<Button>();
    }

    //게임 플레이 중 상시 UI

    public void OnGameUI()
    {
        Logo.GetComponent<Image>().sprite = null;
        Logo.SetActive(true);
        SoundBt.interactable = true;

    }
 
    //이벤트성 

    //유저 WorkStation 입장 시 
    public void SpriteColorChange(SpriteRenderer targetSprite,Color targetColor)
    {
        if (targetSprite != null)
        {
            targetSprite.color = targetColor;
        }
    }

   public void CriminalPopupUI()
    {

    }
    
    public void ArrowUI()
    {
           //유저 길잡이용 
    }


    public void ShowMaxUI(ItemType rock)
    {
        //채굴시 MAX 
    }

    public void EnidngUI()
    {
        //게임 종료 후 팝업 
    }

}
