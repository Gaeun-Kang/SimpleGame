using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JailManager : MonoBehaviour
{
    public static JailManager Instance { get; private set; }

    [Header("감옥 수용 설정")]
    [SerializeField] private int baseCapacity = 20;  // 기본 수용 인원
    [SerializeField] private int expandedCapacity = 80;  // 확장 후 수용 인원 (4칸)

    [Header("감옥 시각 오브젝트")]
    [Tooltip("기본 방 GameObject (항상 활성)")]
    [SerializeField] private GameObject jailRoomBase;

    [Tooltip("확장 시 활성화될 방 3칸 (순서대로)")]
    [SerializeField] private GameObject[] jailRoomExpansions;  // length = 3

 
    private List<CriminalNPC> inmates = new List<CriminalNPC>();
    private int capacity;
    private bool isExpanded;

    public int InmateCount => inmates.Count;
    public int Capacity => capacity;
    public bool IsFull => inmates.Count >= capacity;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        capacity = baseCapacity;

        // 확장 방은 비활성으로 시작
        if (jailRoomExpansions != null)
            foreach (var room in jailRoomExpansions)
                if (room != null) room.SetActive(false);
    }

    
    public void AddInmate(CriminalNPC criminal)
    {
        if (inmates.Contains(criminal)) return;

        inmates.Add(criminal);

        // 수용 인원 UI 갱신
       // UIManager.Instance?.UpdateJailCount(inmates.Count, capacity);

        // 처음으로 가득 찬 시점에만 알림
        if (IsFull && !isExpanded)
        {
            Debug.Log("[JailManager] 감옥 수용 인원 Full → 확장 기능 UI 활성화");
            FeatureManager.Instance?.NotifyJailFull();
        }
    }

    public void ExpandJail()
    {
        if (isExpanded) return;

        isExpanded = true;
        capacity = expandedCapacity;

        // 방 3칸 순차 활성화 (시각적 확장)
        if (jailRoomExpansions != null)
            foreach (var room in jailRoomExpansions)
                if (room != null) room.SetActive(true);

       // UIManager.Instance?.UpdateJailCount(inmates.Count, capacity);
        Debug.Log($"[JailManager] 감옥 확장 완료. 수용 가능 인원 : {capacity}");
    }

}
