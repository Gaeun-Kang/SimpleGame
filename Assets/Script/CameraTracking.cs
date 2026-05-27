using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{

    //ÄõÅÍºäÇ³ Ä«¸Þ¶ó ¼¼ÆÃ 
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;


    private void Update()
    {
        transform.position = target.position + offset; 
    }

}
