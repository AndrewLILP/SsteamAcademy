using UnityEngine;

public class DriveShot : IGolfShotType
{
    public string ShotName => "Drive";
    public string AnimationTrigger => "GolfDrive";

    public void ExecuteShot(Vector3 aimDirection, float power)
    {
        //execute later
        Debug.Log("Executing drive shot");
    }

    public void SetupCamera()
    {
        //implement later
        Debug.Log("Setting up camera");
    }
}
