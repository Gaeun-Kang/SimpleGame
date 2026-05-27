using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorkerNPC : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arrivalThreshold = 0.35f;
    [SerializeField] private float searchInterval = 0.5f;

    private NPCManager npcManager;
    private Transform workStationTransform;
    private NavMeshAgent agent;

    private GameObject targetRock;
    private bool isCarrying = false;

    public void Init(NPCManager manager, Transform workStation)
    {
        npcManager = manager;
        workStationTransform = workStation;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        StartCoroutine(WorkLoop());
    }

    private IEnumerator WorkLoop()
    {
        while (true)
        {
            // ── 1. Rock 탐색 및 이동 ──
            targetRock = FindNearestAvailableRock();

            if (targetRock == null)
            {
                yield return new WaitForSeconds(searchInterval);
                continue;
            }

            // 예약
            if (!npcManager.TryReserveRock(targetRock))
            {
                targetRock = null;
                yield return new WaitForSeconds(searchInterval);
                continue;
            }

            // Rock으로 이동
            yield return MoveTo(targetRock.transform.position);

            // 도착 후 채집 (비활성화 + ItemManager에 리스폰 알림)
            if (targetRock != null && targetRock.activeSelf)
            {
                targetRock.SetActive(false);
                ItemManager.Instance?.NotifyWorkerCollected(targetRock);

                // ── 2. WorkStation으로 이동 ──
                isCarrying = true;
                yield return MoveTo(workStationTransform.position);

                // WorkStation에 Rock 투입
                var ws = workStationTransform.GetComponent<WorkStation>();
                ws?.DepositRockFromWorker(targetRock);

                isCarrying = false;
            }

            npcManager.ReleaseRock(targetRock);
            targetRock = null;

            yield return new WaitForSeconds(searchInterval);
        }
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);

        // 경로 계산 대기
        yield return null;

        while (true)
        {
            if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
                break;
            yield return null;
        }

        agent.ResetPath();
    }

    // ─────────────────────────────────────────
    // MineArea 내 최근접 미예약 Rock 탐색
    // ─────────────────────────────────────────

    private GameObject FindNearestAvailableRock()
    {
        // MineArea Collider 수집
        GameObject[] mineAreaObjs = GameObject.FindGameObjectsWithTag("MineArea");
        var mineColliders = new List<Collider>();
        foreach (var go in mineAreaObjs)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) mineColliders.Add(col);
        }

        // 전체 Rock 탐색
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");

        float minDist = float.MaxValue;
        GameObject nearest = null;

        foreach (var rock in rocks)
        {
            if (!rock.activeSelf) continue;

            // MineArea 내부 여부 확인
            bool inArea = false;
            foreach (var col in mineColliders)
            {
                if (col.bounds.Contains(rock.transform.position))
                {
                    inArea = true;
                    break;
                }
            }
            if (!inArea) continue;

            // 이미 다른 인부가 예약 중인 Rock 제외
            // (실제 예약은 WorkLoop에서 TryReserveRock으로 진행)
            float dist = Vector3.Distance(transform.position, rock.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = rock;
            }
        }

        return nearest;
    }
}
