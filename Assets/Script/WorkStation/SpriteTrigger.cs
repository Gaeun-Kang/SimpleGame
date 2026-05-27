using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTrigger : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();   
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("player ¿‘¿Â");
            UIManager.Instance.SpriteColorChange(spriteRenderer, Color.green);
        }
    }
}
