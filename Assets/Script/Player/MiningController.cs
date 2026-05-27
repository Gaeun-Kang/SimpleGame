using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MiningController : MonoBehaviour
{
    [Header("채굴 모드")]
    [SerializeField] private MiningMode currentMode = MiningMode.Basic;

    [Header("그리드 참조 (ItemManager와 동기화)")]
    [Tooltip("ItemManager Inspector의 gridOrigin과 동일한 값")]
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;
    [SerializeField] private float cellSize = 1.5f;
    [SerializeField] private int gridColumns = 6;
    [SerializeField] private int gridRows = 10;

    // 현재 플레이어가 접촉 중인 Rock 오브젝트 집합
    // (OnTriggerEnter/Exit로 추적 ? 범위 채굴 시 중심 결정에 사용)
    private HashSet<GameObject> touchingRocks = new HashSet<GameObject>();

    // ─────────────────────────────────────────
    // 외부 API
    // ─────────────────────────────────────────

    /// <summary>FeatureManager가 기능 개방 시 호출합니다.</summary>
    public void SetMiningMode(MiningMode mode)
    {
        currentMode = mode;
        Debug.Log($"[MiningController] 채굴 모드 변경 → {mode}");
    }

    // ─────────────────────────────────────────
    // Trigger 감지
    // ─────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Rock")) return;

        touchingRocks.Add(other.gameObject);

        // 접촉한 Rock을 중심으로 현재 모드에 맞는 범위 채굴 실행
        CollectInRange(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Rock")) return;
        touchingRocks.Remove(other.gameObject);
    }
    private void CollectInRange(GameObject centerRock)
    {
        // 중심 Rock의 그리드 좌표 계산
        if (!TryGetGridCoord(centerRock.transform.position, out int centerCol, out int centerRow))
        {
            // 그리드 범위 밖이면 단일 채집으로 fallback
            ItemManager.Instance?.TryCollectRock(centerRock);
            return;
        }

        // 모드별 반경 결정
        // Basic     : 1x1 (자기 자신만)
        // Drill     : 2x2 (중심 포함 최대 4개 ? 중심 기준 0~1 offset)
        // Excavator : 5x5 (중심 기준 -2~2 offset)
        List<Vector2Int> offsets = GetOffsets(currentMode);

        foreach (var offset in offsets)
        {
            int col = centerCol + offset.x;
            int row = centerRow + offset.y;

            if (col < 0 || col >= gridColumns || row < 0 || row >= gridRows) continue;

            // 그리드 위치의 Rock GameObject 조회
            Vector3 targetPos = gridOrigin + new Vector3(col * cellSize, 0f, row * cellSize);
            GameObject rock = FindRockAt(targetPos);
            if (rock == null) continue;

            ItemManager.Instance?.TryCollectRock(rock);
        }
    }
    private bool TryGetGridCoord(Vector3 worldPos, out int col, out int row)
    {
        Vector3 local = worldPos - gridOrigin;

        col = Mathf.RoundToInt(local.x / cellSize);
        row = Mathf.RoundToInt(local.z / cellSize);

        return col >= 0 && col < gridColumns && row >= 0 && row < gridRows;
    }

    /// <summary>
    /// 지정 월드 좌표 근처에 있는 Rock Tag 오브젝트를 찾습니다.
    /// cellSize * 0.4f 반경 내 최근접 오브젝트를 반환합니다.
    /// </summary>
    private GameObject FindRockAt(Vector3 worldPos)
    {
        float searchRadius = cellSize * 0.4f;
        Collider[] hits = Physics.OverlapSphere(worldPos, searchRadius);

        float minDist = float.MaxValue;
        GameObject found = null;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Rock")) continue;
            float dist = Vector3.Distance(hit.transform.position, worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                found = hit.gameObject;
            }
        }
        return found;
    }

    /// <summary>모드별 그리드 offset 목록을 반환합니다.</summary>
    private List<Vector2Int> GetOffsets(MiningMode mode)
    {
        var list = new List<Vector2Int>();

        switch (mode)
        {
            case MiningMode.Basic:
                // 1x1 : 자기 자신만
                list.Add(Vector2Int.zero);
                break;

            case MiningMode.Drill:
                // 2x2 : 중심(0,0) 기준 우측·하단으로 확장
                for (int c = 0; c <= 1; c++)
                    for (int r = 0; r <= 1; r++)
                        list.Add(new Vector2Int(c, r));
                break;

            case MiningMode.Excavator:
                // 5x5 : 중심 기준 -2 ~ +2
                for (int c = -2; c <= 2; c++)
                    for (int r = -2; r <= 2; r++)
                        list.Add(new Vector2Int(c, r));
                break;
        }
        return list;
    }
}
