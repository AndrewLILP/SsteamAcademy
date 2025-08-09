using UnityEngine;
using System.Reflection;

// Simple Creation
public class BridgeFactory
{
    public static GameObject CreateBridge(string bridgeId, BridgeType type, Vector3 position)
    {
        var bridgeObj = new GameObject($"Bridge_{bridgeId}");
        bridgeObj.transform.position = position;

        // add core components
        var bridge = bridgeObj.AddComponent<Bridge>();
        bridgeObj.AddComponent<BridgeDetector>();


    // add collider for detection
        var collider = bridgeObj.AddComponent<BoxCollider> ();
        collider.isTrigger = true;
        collider.size = new Vector3(2f, 1f, 2f);

        // set configuration via reflection 
        var configField = typeof(Bridge).GetField("config",
            BindingFlags.NonPublic | BindingFlags.Instance);
        configField?.SetValue(bridge, new BridgeConfig(bridgeId, type));

        return bridgeObj;
    }
}
