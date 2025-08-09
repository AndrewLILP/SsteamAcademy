using UnityEngine;
using System;

// ==============================================
// MAIN BRIDGE COMPONENT
// ==============================================

public class Bridge : MonoBehaviour, IBridge
{
    [SerializeField] private BridgeConfig config;
    private BridgeState currentState = BridgeState.Available;
    private IBridgeValidator validator;

    // Observer pattern
    public event Action<IBridge, ICrosser> OnBridgeCrossed;

    public string BridgeId => config.bridgeId;
    public BridgeState CurrentState => currentState;

    private void Awake()
    {
        SetupValidator();
    }

    private void SetupValidator()
    {
        // Strategy pattern - simple validator selection
        validator = config.bridgeType switch
        {
            BridgeType.OneTime => new OneTimeValidator(),
            _ => new StandardValidator()
        };
    }

    public bool CanCross(ICrosser crosser)
    {
        return validator.CanCross(this, crosser);
    }

    public bool AttemptCrossing(ICrosser crosser)
    {
        if (!CanCross(crosser))
        {
            Debug.Log($"Cannot cross bridge {BridgeId}");
            return false;
        }

        // Execute crossing
        currentState = BridgeState.Crossing;
        ProcessCrossing(crosser);
        return true;
    }

    private void ProcessCrossing(ICrosser crosser)
    {
        // Update state based on bridge type
        if (config.bridgeType == BridgeType.OneTime)
        {
            currentState = BridgeState.Crossed;
        }
        else
        {
            currentState = BridgeState.Available;
        }

        // Notify observers (UI, puzzle systems)
        OnBridgeCrossed?.Invoke(this, crosser);

        Debug.Log($"Bridge {BridgeId} crossed by {crosser.CrosserId}");
    }
}