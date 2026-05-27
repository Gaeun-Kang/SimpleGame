using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureManager : MonoBehaviour
{
    public static FeatureManager Instance { get; private set; }


    public enum FeatureType
    {
        Drill,
        Excavator,   // 포크레인
        JailExpand
    }



    [Header("드릴")]
    [SerializeField] private int drillCost = 50;
    [SerializeField] private GameObject drillFeatureUI;     // 드릴 기능 개방 UI 오브젝트

    [Header("포크레인")]
    [SerializeField] private int excavatorCost = 120;
    [SerializeField] private GameObject excavatorFeatureUI;

    [Header("감옥 확장")]
    [SerializeField] private int jailExpandCost = 80;
    [SerializeField] private GameObject jailExpandFeatureUI;

    [Header("플레이어 채굴 컴포넌트 참조")]
    [Tooltip("Player에 부착된 MiningController 컴포넌트")]
    [SerializeField] private MiningController miningController;


    private bool isDrillUnlocked;
    private bool isExcavatorUnlocked;
    private bool isJailExpandUnlocked;

    // 각 기능의 현재 누적 투입 재화
    private Dictionary<FeatureType, int> depositedMap = new Dictionary<FeatureType, int>
    {
        { FeatureType.Drill,      0 },
        { FeatureType.Excavator,  0 },
        { FeatureType.JailExpand, 0 }
    };

    // 각 기능의 총 비용
    private Dictionary<FeatureType, int> costMap;

    // 공개 읽기 전용
    public bool IsDrillUnlocked => isDrillUnlocked;
    public bool IsExcavatorUnlocked => isExcavatorUnlocked;

    // ─────────────────────────────────────────
    // 이벤트
    // ─────────────────────────────────────────

    /// <summary>기능이 개방됐을 때 (FeatureType)</summary>
    public event System.Action<FeatureType> OnFeatureUnlocked;

    /// <summary>투입 재화가 변경됐을 때 ? UI 잔액 표시용 (type, remaining)</summary>
    public event System.Action<FeatureType, int> OnDepositChanged;

    // ─────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        costMap = new Dictionary<FeatureType, int>
        {
            { FeatureType.Drill,      drillCost      },
            { FeatureType.Excavator,  excavatorCost  },
            { FeatureType.JailExpand, jailExpandCost }
        };

        // 모든 기능 UI 비활성 상태로 시작
        SetUIActive(drillFeatureUI, false);
        SetUIActive(excavatorFeatureUI, false);
        SetUIActive(jailExpandFeatureUI, false);
    }

    private void OnEnable()
    {
        // 재화 변경 이벤트 구독
        if (ItemManager.Instance != null)
            ItemManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
    }

    private void OnDisable()
    {
        if (ItemManager.Instance != null)
            ItemManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    // ─────────────────────────────────────────
    // 재화 변경 핸들러 ? UI 활성화 타이밍 제어
    // ─────────────────────────────────────────

    private void HandleCurrencyChanged(int newCurrency)
    {
        // 최초 재화 획득 시 드릴 UI 활성화
        if (!isDrillUnlocked && newCurrency > 0)
            SetUIActive(drillFeatureUI, true);

        // 드릴 개방 후 포크레인 UI 활성화
        if (isDrillUnlocked && !isExcavatorUnlocked)
            SetUIActive(excavatorFeatureUI, true);
    }


    /// <summary>
    /// 플레이어가 기능 UI 위에 Overlap될 때 프레임마다 호출합니다.
    /// depositAmount만큼 재화를 차감하고 진행도를 갱신합니다.
    /// 잔여 비용이 없으면 기능을 개방합니다.
    /// </summary>
    public void TryDepositCurrency(FeatureType type, int depositAmount)
    {
        if (IsUnlocked(type)) return;
        if (!ItemManager.Instance.TrySpendCurrency(depositAmount)) return;

        depositedMap[type] += depositAmount;

        int remaining = costMap[type] - depositedMap[type];
        remaining = Mathf.Max(0, remaining);

        // 잔액 UI 갱신
        OnDepositChanged?.Invoke(type, remaining);

        if (depositedMap[type] >= costMap[type])
            Unlock(type);
    }

    private void Unlock(FeatureType type)
    {
        switch (type)
        {
            case FeatureType.Drill:
                isDrillUnlocked = true;
                SetUIActive(drillFeatureUI, false);

                // MiningController에 드릴 모드 적용 (2x2 채굴)
                miningController?.SetMiningMode(MiningMode.Drill);

                // 드릴 개방 → 포크레인·인부 UI 조건 해금
                SetUIActive(excavatorFeatureUI, true);
                NPCManager.Instance?.OnDrillUnlocked();
                break;

            case FeatureType.Excavator:
                isExcavatorUnlocked = true;
                SetUIActive(excavatorFeatureUI, false);

                // 포크레인 모드 적용 (5x5 채굴)
                miningController?.SetMiningMode(MiningMode.Excavator);
                break;

            case FeatureType.JailExpand:
                isJailExpandUnlocked = true;
                SetUIActive(jailExpandFeatureUI, false);
                JailManager.Instance?.ExpandJail();
                break;
        }

        OnFeatureUnlocked?.Invoke(type);
        Debug.Log($"[FeatureManager] {type} 개방 완료");
    }

    /// <summary>JailManager가 수용 인원 Full을 감지하면 호출합니다.</summary>
    public void NotifyJailFull()
    {
        if (isJailExpandUnlocked) return;
        SetUIActive(jailExpandFeatureUI, true);
    }

    public bool IsUnlocked(FeatureType type)
    {
        return type switch
        {
            FeatureType.Drill => isDrillUnlocked,
            FeatureType.Excavator => isExcavatorUnlocked,
            FeatureType.JailExpand => isJailExpandUnlocked,
            _ => false
        };
    }

    /// <summary>기능별 남은 비용 조회 (UI 초기 표시용)</summary>
    public int GetRemaining(FeatureType type)
    {
        return Mathf.Max(0, costMap[type] - depositedMap[type]);
    }

    private void SetUIActive(GameObject ui, bool active)
    {
        if (ui != null) ui.SetActive(active);
    }
}


