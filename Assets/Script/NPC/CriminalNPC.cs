using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//범인 : 체포 - MovetoJail 관련 스크립트 
//체포 시 외관 변경 미구현 

public class CriminalNPC : MonoBehaviour
{
    [SerializeField] private float arrivalThreshold = 0.5f;


    [Header("Nav 관련")]
    [SerializeField] private Transform WaitingPoint;
    [SerializeField] private Transform jailTransform;
    [SerializeField] private NavMeshAgent agent;

    public int RequiredHandcuffs { get; private set; }
    public bool IsBeingArrested { get; private set; }

    private NPCManager npcManager;
    private Renderer[] renderes;

    private bool isWaiting = false;

    void Start()
    {
        MovetoWaitingpoint();    
    }

    public void Init(NPCManager manager, Transform jail)
    {
        npcManager = manager;
        jailTransform = jail;
        agent = GetComponent<NavMeshAgent>();
        //renderers = GetComponentsInChildren<Renderer>();
    }

    //활성화 : 체포 수갑 갯수 지정, NavMesh 활성화 
    public void Activate()
    {
        IsBeingArrested = false;
        RequiredHandcuffs = Random.Range(1, 5);   
        //UI Manager에게서 수갑 popup 받아올 것

        agent.enabled = true;
        agent.isStopped = false;
    }

    private void Update()
    {
        if(!isWaiting && agent.hasPath)
        {
            if(CheckArrival())
            {
               //UI pop up
            }
        }
    }

    private void MovetoWaitingpoint()
    {
        if (WaitingPoint == null)
        {
            Debug.LogWarning("Waiting Point 누락");
            return;
        }

        isWaiting = false;
        agent.isStopped = false;
        agent.SetDestination(WaitingPoint.position);
    }

    private bool CheckArrival()
    {
        if (agent.pathPending) return false;

      
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
        return false;
    }


    public void Arrest()
    {
        if (IsBeingArrested) return;

        IsBeingArrested = true;

       /* // Material 색상 변경 ? MaterialPropertyBlock으로 인스턴스화 없이 처리
        foreach (var r in renderers)
        {
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", arrestedColor);
            r.SetPropertyBlock(mpb);
        }

        StartCoroutine(MoveToJailRoutine());
    
        */
        }


    //Jail로 이동 (코루틴) 
    private IEnumerator MoveToJailRoutine()
    {
        agent.SetDestination(jailTransform.position);
        yield return null;

        while (true)
        {
            if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
                break;
            yield return null;
        }

        agent.ResetPath();
        agent.isStopped = true;

        // JailManager에 수용 알림
        JailManager.Instance?.AddInmate(this);

        // Pool 반환은 JailManager가 만원 등 처리 후 결정
        // 즉시 반환이 필요하면 아래 주석 해제
        // npcManager.ReturnCriminalToPool(this);
    }

}
