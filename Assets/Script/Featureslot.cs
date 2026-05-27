using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Featureslot : MonoBehaviour
{
    [Header("기능 설정")]
    [SerializeField] private FeatureManager.FeatureType featureType;
    [SerializeField] private int depositPerSecond = 5;  // 초당 투입 재화량

    private bool playerInside;

    // ─────────────────────────────────────────
    // Trigger 감지
    // ─────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }

    // ─────────────────────────────────────────
    // 재화 투입 루프
    // ─────────────────────────────────────────

    private void Update()
    {
        if (!playerInside) return;
        if (FeatureManager.Instance == null) return;
        if (FeatureManager.Instance.IsUnlocked(featureType)) return;

        // 프레임 독립적 투입 : 초당 depositPerSecond 만큼 소비
        int deposit = Mathf.CeilToInt(depositPerSecond * Time.deltaTime);
        if (deposit <= 0) deposit = 1;

        FeatureManager.Instance.TryDepositCurrency(featureType, deposit);
    }
}
