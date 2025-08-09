// Puzzle Interface
public interface IPuzzleSystem
{
    void OnBridgeCrossed(IBridge bridge, ICrosser crosser);
    bool IsActive {  get; }
}
