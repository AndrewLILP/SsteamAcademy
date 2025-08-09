using UnityEngine;

public interface IGolfObserver 
{
    void OnGolfStateChanged(GolfState newState);
    void OnShotTypeChanged(IGolfShotType shotType);
    void OnShotsRemainingChanged(int remaining);
}
