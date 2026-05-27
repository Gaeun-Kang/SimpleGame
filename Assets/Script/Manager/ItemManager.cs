using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{

    public static ItemManager Instance { get; private set; }

    [Header("돌 - 그리드 설정")]
    [SerializeField] private int gridColumns = 6;
    [SerializeField] private int gridRows = 10;
    [SerializeField] private float cellSize = 1.5f;
    //그리드가 배치되는 중심점 Area
    [SerializeField] private Transform MineArea;
    [SerializeField] private float RockY = 1.3f;

    [Header("돌 - 프리팹 및 풀")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float rockRespawnDelay = 5f;

    [Header("플레이어 관련")]
    [SerializeField] private int maxRocks = 10;
    [SerializeField] private RockBag rockBag;


    [Header("수갑 - 설정")]
    [SerializeField] private GameObject handcuffPrefab;
    [SerializeField] private Transform handcuffSpawnPoint;    // 기계 산출 위치
    [SerializeField] private int maxHandcuffs = 20;            // 보유 상한 

    [Header("재화 - 설정")]
    [SerializeField] private int currencyPerArrest = 25;       // 체포 1회당 획득량
                                                               
                                                              
    // 돌 오브젝트 풀 : 그리드 인덱스와 1:1 매핑
    private RockSlot[] rockSlots;
    private Dictionary<GameObject, int> rockObjectToSlot = new Dictionary<GameObject, int>();

    // 수갑 풀 (씬에 존재하는 비활성 수갑 오브젝트 목록)
    private Queue<GameObject> handcuffPool = new Queue<GameObject>();
    // 현재 씬에 활성화된 수갑 목록 (플레이어 미수거)
    private List<GameObject> activeHandcuffs = new List<GameObject>();

    // 플레이어 보유 수량
    private int playerRocks;
    private int playerHandcuffs;
    private int playerCurrency;

    public int PlayerRocks => playerRocks;
    public int PlayerHandcuffs => playerHandcuffs;
    public int PlayerCurrency => playerCurrency;
    public int MaxRocks => maxRocks;
    public int MaxHandcuffs => maxHandcuffs;

    public event System.Action<int> OnRocksChanged;
    public event System.Action<int> OnHandcuffsChanged;
    public event System.Action<int> OnCurrencyChanged;
 
    private class RockSlot
    {
        public Vector3 WorldPosition;
        public GameObject RockObject;
        public bool IsActive;       
        public bool IsRespawning;   // 리스폰 코루틴 진행 중인가
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {

        if (rockBag != null)
            rockBag.Init(maxRocks);
        else Debug.Log("RockBag 누락");

        InitRockGrid();
        PrewarmHandcuffPool(10); // 초기 풀 크기
    }

    /// <summary>6x10 그리드에 돌 오브젝트를 배치하고 슬롯을 초기화합니다.</summary>
    private void InitRockGrid()
    {
        int total = gridColumns * gridRows;
        rockSlots = new RockSlot[total];

        float gridWidth = (gridColumns - 1) * cellSize;
        float gridDepth = (gridRows - 1) * cellSize;

        Vector3 startOrigin = MineArea.position - new Vector3(gridWidth / 2f, 0f, gridDepth / 2);

        for (int i = 0; i < total; i++)
        {
            int col = i % gridColumns;
            int row = i / gridColumns;

            Vector3 pos = startOrigin + new Vector3(col * cellSize, RockY, row * cellSize);

            GameObject rock = Instantiate(rockPrefab, pos, Quaternion.identity);
            rock.name = $"Rock_{i}";

            // RockPickup 컴포넌트에 슬롯 인덱스 전달
            var pickup = rock.GetComponent<RockPickup>();
            if (pickup != null) pickup.Init(i);

            rockSlots[i] = new RockSlot
            {
                WorldPosition = pos,
                RockObject = rock,
                IsActive = true,
                IsRespawning = false
            };

            rockObjectToSlot[rock] = i;
        }
    }

    /// <summary>수갑 오브젝트를 미리 생성해 풀에 적재합니다.</summary>
    private void PrewarmHandcuffPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(handcuffPrefab);
            obj.SetActive(false);
            handcuffPool.Enqueue(obj);
        }
    }

    //RockPickup 컴포넌트가 플레이어 충돌 시 호출합니다.
    //보유량이 MAX면 UIManager에 알리고 채집 방지

    public bool TryCollectRock(GameObject rockObject)
    {
        if (!rockObjectToSlot.TryGetValue(rockObject, out int slotIndex)) return false;

        RockSlot slot = rockSlots[slotIndex];
        if (!slot.IsActive) return false;

        if (playerRocks >= maxRocks)
        {
            // 채집 불가 → MAX UI 요청 (UIManager 전담)
           UIManager.Instance?.ShowMaxUI(ItemType.Rock);
            return false;
        }

        // 채집 성공
        slot.IsActive = false;
        slot.RockObject.SetActive(false);

        playerRocks++;
        OnRocksChanged?.Invoke(playerRocks);

        rockBag?.CarryRock();

        // 리스폰 예약
        if (!slot.IsRespawning)
            StartCoroutine(RespawnRockRoutine(slotIndex));

        return true;
    }

    //지정 슬롯의 돌을 5초 후 같은 자리에 재생성합니다. (Pool 재사용)

    private IEnumerator RespawnRockRoutine(int slotIndex)
    {
        RockSlot slot = rockSlots[slotIndex];
        slot.IsRespawning = true;

        yield return new WaitForSeconds(rockRespawnDelay);

        slot.RockObject.transform.position = slot.WorldPosition;
        slot.RockObject.SetActive(true);
        slot.IsActive = true;
        slot.IsRespawning = false;
        Debug.Log("재생성 완료");
    }

    /// WorkStation이 호출합니다.
    /// 플레이어 돌 1개를 소비하고 수갑을 지정 위치에 산출합니다.
    public bool TryConsumeRocksForHandcuff()
    {
        if (playerRocks <= 0) return false;

        // 돌 차감
        playerRocks--;
        OnRocksChanged?.Invoke(playerRocks);
        //돌소모 함수 
        rockBag?.minusRock();

        // 수갑 산출 위치 결정
        Vector3 spawnPos = handcuffSpawnPoint != null
                           ? handcuffSpawnPoint.position
                           : Vector3.zero;

        SpawnHandcuff(spawnPos);
        return true;
    }


    private void SpawnHandcuff(Vector3 position)
    {
        GameObject hc;

        if (handcuffPool.Count > 0)
        {
            hc = handcuffPool.Dequeue();
        }
        else
        {
            hc = Instantiate(handcuffPrefab);
        }

        hc.transform.position = position;
        hc.SetActive(true);

        // HandcuffPickup 컴포넌트에 풀 반환 콜백 연결
        var pickup = hc.GetComponent<Handcuffpickup>();
        if (pickup != null) pickup.Init(this);

        activeHandcuffs.Add(hc);
    }

    public bool TryCollectHandcuff(GameObject handcuffObj)
    {
        if (!activeHandcuffs.Contains(handcuffObj)) return false;

        if (playerHandcuffs >= maxHandcuffs)
        {
            //UIManager.Instance?.ShowMaxUI(ItemType.Handcuff);
            Debug.Log("수갑이 상한선만큼 모였습니다");
            return false;
        }

        // 수거 성공
        activeHandcuffs.Remove(handcuffObj);
        handcuffObj.SetActive(false);
        handcuffPool.Enqueue(handcuffObj);

        playerHandcuffs++;
        OnHandcuffsChanged?.Invoke(playerHandcuffs);
        return true;
    }

    /// <summary>
    /// 체포 시 수갑을 소비합니다. (ArrestSystem 호출)
    /// </summary>
    public bool TryConsumeHandcuffs(int amount)
    {
        if (playerHandcuffs < amount) return false;

        playerHandcuffs -= amount;
        OnHandcuffsChanged?.Invoke(playerHandcuffs);
        return true;
    }

    /// 체포 완료 시 ArrestSystem이 호출합니다.
    /// count = 체포한 범인 수 (보통 1)

    public void AddCurrency(int criminalCount = 1)
    {
        playerCurrency += currencyPerArrest * criminalCount;
        OnCurrencyChanged?.Invoke(playerCurrency);
    }
  
    /// FeatureManager가 기능 개방 비용을 차감할 때 호출합니다.

    public bool TrySpendCurrency(int amount)
    {
        if (playerCurrency < amount) return false;

        playerCurrency -= amount;
        OnCurrencyChanged?.Invoke(playerCurrency);
        return true;
    }

    //NPC 전용 메서드
    //WorkerNPC 전용 

    public void ProduceHandcuffFromWorker()
    {
        Vector3 spawnPos = handcuffSpawnPoint != null
                           ? handcuffSpawnPoint.position
                           : Vector3.zero;
        SpawnHandcuff(spawnPos);
    }

    public void NotifyWorkerCollected(GameObject rockObject)
    {
        if (!rockObjectToSlot.TryGetValue(rockObject, out int slotIndex)) return;

        RockSlot slot = rockSlots[slotIndex];
        slot.IsActive = false;

        if (!slot.IsRespawning)
            StartCoroutine(RespawnRockRoutine(slotIndex));
    }

    //PoliceNPC 전용 
    public int CollectHandcuffsAtOutput()
    {
        if (handcuffSpawnPoint == null) return 0;

        float pickupRadius = 1.0f;
        int collected = 0;

        // 역방향 순회 (Remove 안전)
        for (int i = activeHandcuffs.Count - 1; i >= 0; i--)
        {
            var hc = activeHandcuffs[i];
            if (hc == null || !hc.activeSelf) continue;

            float dist = Vector3.Distance(hc.transform.position, handcuffSpawnPoint.position);
            if (dist <= pickupRadius)
            {
                activeHandcuffs.RemoveAt(i);
                hc.SetActive(false);
                handcuffPool.Enqueue(hc);
                collected++;
            }
        }

        return collected;
    }
}
