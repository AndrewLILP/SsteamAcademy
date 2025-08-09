using UnityEngine;

public enum BridgeState
{
    Available,
    Crossed, // for one-time bridges
    Crossing // temp state during crossing
}

public enum BridgeType
{
    Standard,
    OneTime
}

[System.Serializable]
public struct BridgeConfig
{
    public string bridgeId;
    public BridgeType bridgeType;

    public BridgeConfig(string id, BridgeType type = BridgeType.Standard)
    {
        bridgeId = id;
        bridgeType = type;
    }

}