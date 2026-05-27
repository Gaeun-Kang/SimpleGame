using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    // ─────────────────────────────────────────
    // Inspector 설정
    // ─────────────────────────────────────────

    [Header("인부 설정")]
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private int maxWorkers = 3;
    [SerializeField] private Transform workerSpawnPoint;
    [SerializeField] private Transform workStationTransform;   // Rock 이동 목적지
    [SerializeField] private int workerHireCost = 40;
    [SerializeField] private GameObject workerHireUI;

    [Header("경찰관(자동 체포) 설정")]
    [SerializeField] private GameObject PoliceNPCPrefab;
    [SerializeField] private int maxPoliceNPCs = 2;
    [SerializeField] private Transform arrestNPCSpawnPoint;
    [SerializeField] private Transform workStationOutputTransform; // Handcuff 산출 위치
    [SerializeField] private int PoliceNPCHireCost = 60;
    [SerializeField] private GameObject arrestNPCHireUI;

    [Header("범인 설정")]
    [SerializeField] private GameObject criminalPrefab;
    [SerializeField] private int criminalPoolSize = 30;
    [SerializeField] private Transform arrestAreaTransform;   // 범인 대기 구역 중심
    [SerializeField] private float arrestAreaRadius = 5f;
    [SerializeField] private Transform jailTransform;         // 체포 후 이동 목적지


    // 고용된 인부 목록
    private List<WorkerNPC> activeWorkers = new List<WorkerNPC>();
    // 고용된 자동 경찰관 목록
    private List<PoliceNPC> activeArrestNPCs = new List<PoliceNPC>();

    // 범인 풀
    private Queue<CriminalNPC> criminalPool = new Queue<CriminalNPC>();
    private List<CriminalNPC> activeCriminals = new List<CriminalNPC>();

    // 개방 상태
    private bool isWorkerUnlocked;
    private bool isArrestNPCUnlocked;

    // 인부들이 현재 목표 중인 Rock (중복 타겟 방지)
    private HashSet<GameObject> reservedRocks = new HashSet<GameObject>();


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        InitCriminalPool();

        SetUIActive(workerHireUI, false);
        SetUIActive(arrestNPCHireUI, false);
    }

    private void InitCriminalPool()
    {
        for (int i = 0; i < criminalPoolSize; i++)
        {
            var obj = Instantiate(criminalPrefab);
            var npc = obj.GetComponent<CriminalNPC>();
            npc.Init(this, jailTransform);
            obj.SetActive(false);
            criminalPool.Enqueue(npc);
        }

        // 초기 범인 씬 세팅 
        SpawnCriminal(8);
    }

    /// <summary>FeatureManager가 드릴 개방 시 호출합니다.</summary>
    public void OnDrillUnlocked()
    {
        SetUIActive(workerHireUI, true);
    }

    /// <summary>인부 고용 완료 후 경찰관 고용 UI 활성화.</summary>
    private void OnWorkerHired()
    {
        if (!isArrestNPCUnlocked)
            SetUIActive(arrestNPCHireUI, true);
    }

    public void HireWorker()
    {
        if (activeWorkers.Count >= maxWorkers) return;
        if (!ItemManager.Instance.TrySpendCurrency(workerHireCost)) return;

        var obj = Instantiate(workerPrefab, workerSpawnPoint.position, Quaternion.identity);
        var npc = obj.GetComponent<WorkerNPC>();
        npc.Init(this, workStationTransform);
        activeWorkers.Add(npc);

        isWorkerUnlocked = true;
        OnWorkerHired();

        if (activeWorkers.Count >= maxWorkers)
            SetUIActive(workerHireUI, false);

        Debug.Log($"[NPCManager] 인부 고용 ({activeWorkers.Count}/{maxWorkers})");
    }

    //경찰관 고용 
    public void HirePoliceNPC()
    {
        if (activeArrestNPCs.Count >= maxPoliceNPCs) return;
        if (!ItemManager.Instance.TrySpendCurrency(PoliceNPCHireCost)) return;

        var obj = Instantiate(PoliceNPCPrefab, arrestNPCSpawnPoint.position, Quaternion.identity);
        var npc = obj.GetComponent<PoliceNPC>();
        npc.Init(workStationOutputTransform);
        activeArrestNPCs.Add(npc);

        isArrestNPCUnlocked = true;

        if (activeArrestNPCs.Count >= maxPoliceNPCs)
            SetUIActive(arrestNPCHireUI, false);

        Debug.Log($"[NPCManager] 자동 경찰관 고용 ({activeArrestNPCs.Count}/{maxPoliceNPCs})");
    }

    
    public void SpawnCriminal(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            if (criminalPool.Count == 0) break;

            var npc = criminalPool.Dequeue();

            // ArrestArea 내 랜덤 위치 배치
            Vector2 rnd = Random.insideUnitCircle * arrestAreaRadius;
            npc.transform.position = arrestAreaTransform.position
                                     + new Vector3(rnd.x, 0f, rnd.y);
            npc.gameObject.SetActive(true);
            npc.Activate();
            activeCriminals.Add(npc);
        }
    }

    /// <summary>체포 완료 시 CriminalNPC가 호출합니다.</summary>
    public void ReturnCriminalToPool(CriminalNPC npc)
    {
        activeCriminals.Remove(npc);
        npc.gameObject.SetActive(false);
        criminalPool.Enqueue(npc);
    }

    /// <summary>현재 대기 중인(체포되지 않은) 범인 목록 반환 (ArrestNPC용).</summary>
    public List<CriminalNPC> GetActiveCriminals() => activeCriminals;

    //인부 중복 타켓팅 방지 
    public bool TryReserveRock(GameObject rock)
    {
        if (reservedRocks.Contains(rock)) return false;
        reservedRocks.Add(rock);
        return true;
    }

    public void ReleaseRock(GameObject rock)
    {
        reservedRocks.Remove(rock);
    }

    private void SetUIActive(GameObject ui, bool active)
    {
        if (ui != null) ui.SetActive(active);
    }
}
