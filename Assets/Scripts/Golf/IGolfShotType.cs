using UnityEngine;

public interface IGolfShotType
{
    string ShotName { get; }
    string AnimationTrigger {  get; }
    void ExecuteShot(Vector3 aimDirection, float power);
    void SetupCamera();
}
