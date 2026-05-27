using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//돌 -> 수갑 
public class WorkStation : MonoBehaviour
{
    [Header("플레이어 작업 설정")]
    [Tooltip("돌 1개 → 수갑 1개 변환 시간 (초)")]
    [SerializeField] private float workDuration = 2f;

    [Header("Rock 투입구 위치")]
    [Tooltip("인부가 Rock을 날려 보낼 목적지 Transform")]
    [SerializeField] private Transform rockInputPoint;

    [Header("Slerp 투입 애니메이션")]
    [Tooltip("Rock이 날아오는 호의 최고 높이 (월드 단위)")]
    [SerializeField] private float arcHeight = 2.5f;
    [Tooltip("Rock이 투입구까지 날아오는 시간 (초)")]
    [SerializeField] private float arcDuration = 0.6f;


    private Coroutine workRoutine;
    private bool playerInside;

    // 현재 날아오는 Rock 코루틴 큐 (동시에 여러 개 허용)
    private readonly List<Coroutine> flyingRocks = new List<Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        Debug.Log("수갑 생산 준비");
        if (workRoutine == null)
            workRoutine = StartCoroutine(PlayerWorkRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        StopPlayerWork();
    }

    //UIManager OFF
    private IEnumerator PlayerWorkRoutine()
    {
        while (playerInside)
        {
            if (ItemManager.Instance == null || ItemManager.Instance.PlayerRocks <= 0)
            {
                Debug.Log("안되나요?");
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            // UIManager.Instance?.ShowWorkProgress(gameObject, 0f);

            float elapsed = 0f;
            while (elapsed < workDuration)
            {
                elapsed += Time.deltaTime;
                //  UIManager.Instance?.UpdateWorkProgress(gameObject, Mathf.Clamp01(elapsed / workDuration));
                yield return null;
            }

            bool success = ItemManager.Instance.TryConsumeRocksForHandcuff();
            
            /*
            if (success)
               UIManager.Instance?.CompleteWorkProgress(gameObject);
            else
                        UIManager.Instance?.HideWorkProgress(gameObject);
            */
                        yield return new WaitForSeconds(0.1f);
        }

        workRoutine = null;
        // UIManager.Instance?.HideWorkProgress(gameObject);
    }

    private void StopPlayerWork()
    {
        if (workRoutine != null)
        {
            StopCoroutine(workRoutine);
            workRoutine = null;
        }
        //UIManager.Instance?.HideWorkProgress(gameObject);
    }

 
    /// <summary>
    /// WorkerNPC가 WorkStation 위치에 도착한 뒤 호출합니다.
    /// rockObject : WorkerNPC가 채집해 비활성화했던 Rock GameObject.
    /// Rock을 다시 활성화하여 Slerp 호로 날아오게 한 뒤,
    /// 투입구에 도달하면 수갑 1개를 생산합니다.
    /// </summary>
    public void DepositRockFromWorker(GameObject rockObject)
    {
        if (rockObject == null) return;

        Transform target = rockInputPoint != null ? rockInputPoint : transform;

        var c = StartCoroutine(FlyRockToStation(rockObject, target));
        flyingRocks.Add(c);
    }


    /// <summary>
    /// Rock GameObject를 출발 위치에서 투입구까지 구형 호(Slerp) 경로로 이동시킵니다.
    ///
    /// 구현 방식:
    ///   Slerp는 방향 벡터 기반이므로, 여기서는 "중간 호 정점(midArc)"을 구한 뒤
    ///   start→midArc, midArc→end 두 구간을 각각 Lerp로 연결하는
    ///   Quadratic Bezier 방식을 사용합니다.
    ///   → 화면에서 포물선처럼 보이되 계산이 명확합니다.
    ///   순수 Slerp가 필요하다면 Vector3.Slerp(startDir, endDir, t)로 대체 가능합니다.
    /// </summary>
    private IEnumerator FlyRockToStation(GameObject rock, Transform destination)
    {
        // 날아오기 시작 전 Rock 재활성화 (시각적으로 보여야 하므로)
        rock.SetActive(true);

        // Collider 비활성화 — 날아오는 도중 플레이어가 재채집하는 것을 방지
        var col = rock.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Vector3 startPos = rock.transform.position;
        Vector3 endPos = destination.position;

        // Bezier 제어점 : 두 지점의 중간에서 arcHeight만큼 위
        Vector3 midPoint = (startPos + endPos) * 0.5f + Vector3.up * arcHeight;

        float elapsed = 0f;
        while (elapsed < arcDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / arcDuration);

            // Quadratic Bezier : B(t) = (1-t)²·P0 + 2(1-t)t·P1 + t²·P2
            float u = 1f - t;
            Vector3 pos = (u * u * startPos)
                        + (2f * u * t * midPoint)
                        + (t * t * endPos);

            rock.transform.position = pos;

            // 이동 방향으로 회전 (선택 — 자연스러운 텀블링 효과)
            if (elapsed > Time.deltaTime)
            {
                Vector3 dir = (pos - rock.transform.position).normalized;
                if (dir != Vector3.zero)
                    rock.transform.rotation = Quaternion.LookRotation(dir);
            }

            yield return null;
        }

        // 투입 완료 — Rock 비활성화 (리스폰은 ItemManager가 관리)
        rock.transform.position = endPos;
        rock.SetActive(false);

        // Collider 원상복구 (리스폰 후 다시 활성화될 때를 위해)
        if (col != null) col.enabled = true;

        // 수갑 생산
        ItemManager.Instance?.ProduceHandcuffFromWorker();

        flyingRocks.RemoveAll(c => c == null);
    }


}
