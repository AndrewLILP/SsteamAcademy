using System;

// ==============================================
// BRIDGE INTERFACE
// ==============================================

public interface IBridge
{
    string BridgeId { get; }
    BridgeState CurrentState { get; }
    bool CanCross(ICrosser crosser);
    bool AttemptCrossing(ICrosser crosser);

    // Observer pattern for UI notifications
    event Action<IBridge, ICrosser> OnBridgeCrossed;
}