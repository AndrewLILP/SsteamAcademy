using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

// Observer Pattern

public class BridgeUIManager : MonoBehaviour, IPuzzleSystem
{
    [SerializeField] private TextMeshProUGUI bridgeCountText;
    [SerializeField] private TextMeshProUGUI alertText;

    private int bridgesCrossed = 0;
    private List<string> crossedBridgeIds = new List<string>();

    public bool IsActive => true;

    void Start()
    {
        UpdateUI();

        // register with all bridges
        //var bridges = FindObjectsOfType<Bridge>();
        var bridges = FindObjectsByType<Bridge>(FindObjectsSortMode.None);
        foreach (var bridge in bridges)
        {
            bridge.OnBridgeCrossed += OnBridgeCrossed;
        }
    }

    public void OnBridgeCrossed(IBridge bridge, ICrosser crosser)
    {
        // only count if not already crossed
        if (!crossedBridgeIds.Contains(bridge.BridgeId))
        {
            bridgesCrossed++;
            crossedBridgeIds.Add(bridge.BridgeId);

            ShowAlert($"Bridge {bridge.BridgeId} crossed");
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (bridgeCountText != null)
        {
            bridgeCountText.text = $"Bridges Crossed: {bridgesCrossed}";
        }
    }

    private void ShowAlert(string message)
    {
        if (alertText != null)
        {
            alertText.text = message;
            StartCoroutine(HideAlertAfterDelay(2f));
        }
        Debug.Log(message);
    }

    private IEnumerator HideAlertAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (alertText != null)
        {
            alertText.text = "";
        }
    }
}
