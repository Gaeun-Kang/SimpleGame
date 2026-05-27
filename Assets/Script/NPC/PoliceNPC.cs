using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PoliceNPC : MonoBehaviour
{
    //기능 요약 : 
    //추후 이름 PoliceNPC로 변경 

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arrivalThreshold = 0.35f;

    private Transform handcuffOutputTransform;
    private NavMeshAgent agent;

    // 보유 수갑 수
    private int heldHandcuffs;

    public void Init(Transform handcuffOutput)
    {
        handcuffOutputTransform = handcuffOutput;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        StartCoroutine(ArrestLoop());
    }


    private IEnumerator ArrestLoop()
    {
        while (true)
        {
            // ── 1. 수갑 수거 ──
            if (heldHandcuffs <= 0)
            {
                // Handcuff 산출 위치로 이동
                yield return MoveTo(handcuffOutputTransform.position);

                // 해당 위치 Handcuff 수거 시도
                int collected = ItemManager.Instance?.CollectHandcuffsAtOutput() ?? 0;
                heldHandcuffs += collected;

                if (heldHandcuffs <= 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
            }

            // ── 2. 체포 가능한 범인 탐색 ──
            CriminalNPC target = FindArrestableTarget();
            if (target == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // 범인에게 이동
            yield return MoveTo(target.transform.position);

            // 수갑 요구량 확인 및 체포
            int required = target.RequiredHandcuffs;
            if (heldHandcuffs >= required)
            {
                heldHandcuffs -= required;
                target.Arrest();
                ItemManager.Instance?.AddCurrency();
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    // ─────────────────────────────────────────
    // 체포 가능 범인 탐색
    // ─────────────────────────────────────────

    private CriminalNPC FindArrestableTarget()
    {
        var criminals = NPCManager.Instance?.GetActiveCriminals();
        if (criminals == null || criminals.Count == 0) return null;

        CriminalNPC nearest = null;
        float minDist = float.MaxValue;

        foreach (var c in criminals)
        {
            if (c == null || !c.gameObject.activeSelf) continue;
            if (c.IsBeingArrested) continue;
            if (c.RequiredHandcuffs > heldHandcuffs) continue;  // 수갑 부족 시 건너뜀

            float dist = Vector3.Distance(transform.position, c.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = c;
            }
        }

        return nearest;
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
        yield return null;

        while (true)
        {
            if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
                break;
            yield return null;
        }

        agent.ResetPath();
    }

}
