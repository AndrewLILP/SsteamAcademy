// Strategy Pattern
public class StandardValidator : IBridgeValidator
{
    public bool CanCross(IBridge bridge, ICrosser crosser)
    {
        return bridge.CurrentState == BridgeState.Available;
    }
}

public class OneTimeValidator : IBridgeValidator
{
    public bool CanCross(IBridge bridge, ICrosser crosser)
    {
        return bridge.CurrentState == BridgeState.Available;
    }
}


