using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{

    //모바일게임 쿼터뷰로 만들것 
    [SerializeField] private Transform target;
    private Transform tr;
    void Start()
    {
        tr = GetComponent<Transform>();
 
    }

    void LateUpdate()
    {
        tr.position = new Vector3(tr.position.x + target.transform.position.x, tr.position.y, tr.position.z);
        tr.LookAt(target);
    }
}
