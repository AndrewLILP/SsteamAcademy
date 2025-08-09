using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
// (Observer Pattern)
public class SimpleMissionSystem : MonoBehaviour, IPuzzleSystem
{
    [SerializeField] private string[] requiredBridges;
    [SerializeField] private TextMeshProUGUI missionText;

    private HashSet<string> crossedBridges = new HashSet<string>();

    public bool IsActive => true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateMissionUI();
        // register with bridges
        //var bridges = FindObjectsOfType<Bridge>();
        var bridges = FindObjectsByType<Bridge>(FindObjectsSortMode.None);
        foreach (var bridge in bridges)
        {
            bridge.OnBridgeCrossed += OnBridgeCrossed;
        }

    }

    public void OnBridgeCrossed(IBridge bridge, ICrosser crosser)
    {
        crossedBridges.Add(bridge.BridgeId);
        UpdateMissionUI();

        CheckMissionComplete();
    }
    private void UpdateMissionUI()
    {
        if (missionText != null)
        {
            missionText.text = $"Mission: Cross all bridges\nProgress: {crossedBridges.Count}/{requiredBridges.Length}";
        }
    }

    private void CheckMissionComplete()
    {
        bool allCrossed = requiredBridges.All(bridgeId => crossedBridges.Contains(bridgeId));

        if (allCrossed)
        {
            Debug.Log("Mission Complete!");
            if (missionText != null)
            {
                missionText.text = "Mission Complete! All bridges crossed.";
            }
        }

    }
}
