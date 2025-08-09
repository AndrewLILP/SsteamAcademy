using UnityEngine;

// ==============================================
// BRIDGE DETECTOR COMPONENT
// ==============================================

public class BridgeDetector : MonoBehaviour
{
    private IBridge bridge;

    private void Awake()
    {
        bridge = GetComponent<IBridge>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var crosser = other.GetComponent<ICrosser>();
        if (crosser != null)
        {
            bool crossingSuccessful = bridge?.AttemptCrossing(crosser) ?? false;
            if (!crossingSuccessful)
            {
                // Optional: Handle failed crossing attempt
                Debug.Log($"Failed to cross bridge: {bridge?.BridgeId}");
            }
        }
    }
}